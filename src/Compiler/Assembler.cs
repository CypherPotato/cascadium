﻿using Cascadium.Entity;
using Cascadium.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cascadium.Compiler;

class Assembler
{
    public CompilationContext Context { get; set; }

    public Assembler(CompilationContext context)
    {
        Context = context;
    }

    public CssStylesheet AssemblyCss(FlatStylesheet flatStylesheet)
    {
        CssStylesheet result = new CssStylesheet()
        {
            Options = Context.Options
        };
        int ruleIndex = 0;

        foreach (FlatRule rule in flatStylesheet.Rules)
        {
            if (rule.Declarations.Count == 0)
            {
                // skip empty rules
                continue;
            }

            CssRule cssRule;
            if (rule.IsRuleAtRule()) // checks if the rule is an @ rule, like @font-face
            {
                cssRule = new CssRule()
                {
                    _declarations = rule.Declarations,
                    Selector = BuildCssSelector(rule.Selectors),
                    _order = ++ruleIndex
                };
                result._rules.Add(cssRule);
            }
            else
            {
                string? atRule = rule.PopAtRule();
                string selector = BuildCssSelector(rule.Selectors);

                cssRule = new CssRule()
                {
                    _declarations = rule.Declarations,
                    Selector = selector,
                    _order = ++ruleIndex
                };

                if (atRule != null)
                {
                    bool canMerge = Context.Options.Merge.HasFlag(MergeOption.AtRules) || AtRule.IsGroupAtRule(atRule);
                    CssStylesheet atRuleStylesheet = result.GetOrCreateStylesheet(atRule, canMerge);
                    atRuleStylesheet._rules.Add(cssRule);
                }
                else
                {
                    result._rules.Add(cssRule);
                }
            }
        }

        result._statements.AddRange(flatStylesheet.Statements);

        if (Context.Options.Merge != MergeOption.None)
        {
            Merge(result, Context.Options);
            foreach (CssStylesheet subCss in result._stylesheets)
            {
                Merge(subCss, Context.Options);
            }
        }

        return result;
    }

    string BuildCssSelector(IList<string[]> selectors)
    {
        int flatCount = selectors.Count;

        if (flatCount == 0)
        {
            return "";
        }
        if (flatCount == 1)
        {
            return BuildCssSelector(selectors[0], Array.Empty<string>());
        }
        else
        {
            string carry = BuildCssSelector(selectors[0], Array.Empty<string>());
            for (int i = 1; i < flatCount; i++)
            {
                string[] current = selectors[i];
                if (current.Length == 0) continue;
                carry = BuildCssSelector(current, Helper.SafeSplit(carry, ','));
            }
            return carry;
        }
    }

    string BuildCssSelector(string[] currentSelectors, string[] parentSelectors)
    {
        StringBuilder sb = new StringBuilder();
        if (parentSelectors.Length == 0)
        {
            foreach (string cSelector in currentSelectors)
            {
                string prepared = Helper.PrepareSelectorUnit(cSelector, Context.Options.KeepNestingSpace, Context.Options.Pretty);
                sb.Append(prepared);
                sb.Append(',');
                if (Context.Options.Pretty)
                    sb.Append(' ');
            }
            goto finish;
        }

        foreach (string C in currentSelectors)
        {
            string c = C.Trim();
            foreach (string B in parentSelectors)
            {
                string b = B.Trim();
                string s;

                if (c.StartsWith('&'))
                {
                    sb.Append(b);
                    s = c.Substring(1);
                    if (!Context.Options.KeepNestingSpace)
                    {
                        s = s.TrimStart();
                    }
                }
                else if (c.Contains('&'))
                {
                    string repl = Helper.SafeStrReplace(c, '&', b);
                    s = repl;
                    ;
                }
                else
                {
                    sb.Append(b);
                    if (c.Length != 0 && !Token.Sel_Combinators.Contains(c[0]))
                    {
                        sb.Append(' ');
                    }
                    s = c;
                }

                s = Helper.PrepareSelectorUnit(s, Context.Options.KeepNestingSpace, Context.Options.Pretty);
                sb.Append(s);
                sb.Append(',');
                if (Context.Options.Pretty)
                    sb.Append(' ');
            }
        }

    finish:
        if (sb.Length > 0) sb.Length--;
        if (sb.Length > 0 && Context.Options.Pretty) sb.Length--;
        return sb.ToString();
    }

    public void Merge(CssStylesheet stylesheet, CascadiumOptions options)
    {
        if (options.Merge.HasFlag(MergeOption.Selectors))
        {
            List<CssRule> newRules = [];

            foreach (CssRule rule in stylesheet._rules)
            {
                if (AtRule.IsNotEligibleToSelectorMerge(rule.Selector))
                {
                    newRules.Add(rule);
                    continue;
                }

                CssRule? existingRule = newRules
                    .FirstOrDefault(r => Helper.IsSelectorsEqual(r.Selector, rule.Selector));

                if (existingRule == null)
                {
                    newRules.Add(rule);
                }
                else
                {
                    foreach (var prop in rule._declarations)
                    {
                        existingRule._declarations[prop.Key] = prop.Value;

                        if (options.MergeOrderPriority == MergeOrderPriority.PreserveLast)
                        {
                            if (rule._order > existingRule._order)
                                existingRule._order = rule._order;
                        }
                    }
                }
            }

            stylesheet._rules = newRules;
        }

        if (options.Merge.HasFlag(MergeOption.Declarations))
        {
            // merge top-level only
            List<CssRule> newRules = [];

            foreach (CssRule rule in stylesheet._rules)
            {
                CssRule? existingRule = newRules
                    .FirstOrDefault(r => r.GetHashCode() == rule.GetHashCode());

                if (existingRule == null)
                {
                    newRules.Add(rule);
                }
                else
                {
                    existingRule.Selector = Helper.CombineSelectors(existingRule.Selector, rule.Selector, options.Pretty);
                }
            }

            stylesheet._rules = newRules;
        }
    }
}
