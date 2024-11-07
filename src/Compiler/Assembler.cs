using Cascadium.Entity;
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
        this.Context = context;
    }

    public CssStylesheet AssemblyCss(FlatStylesheet flatStylesheet)
    {
        CssStylesheet result = new CssStylesheet()
        {
            Options = this.Context.Options
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
                    Selector = this.BuildCssSelector(rule.Selectors),
                    _order = ++ruleIndex
                };
                result._rules.Add(cssRule);
            }
            else
            {
                string? atRule = rule.PopAtRule();
                string selector = this.BuildCssSelector(rule.Selectors);

                cssRule = new CssRule()
                {
                    _declarations = rule.Declarations,
                    Selector = selector,
                    _order = ++ruleIndex
                };

                if (atRule != null)
                {
                    bool canMerge = this.Context.Options.Merge.HasFlag(MergeOption.AtRules)
                        || IsGroupAtRule(atRule);
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

        if (this.Context.Options.Merge != MergeOption.None)
        {
            this.Merge(result, this.Context.Options);
            foreach (CssStylesheet subCss in result._stylesheets)
            {
                this.Merge(subCss, this.Context.Options);
            }
        }

        return result;
    }

    static bool IsNotEligibleToSelectorMerge(string selector)
        => selector.StartsWith("@font-face", StringComparison.InvariantCultureIgnoreCase) ||
           selector.StartsWith("@counter-style", StringComparison.InvariantCultureIgnoreCase) ||
           selector.StartsWith("@color-profile", StringComparison.InvariantCultureIgnoreCase);

    static bool IsGroupAtRule(string atRule)
        => atRule.StartsWith("@media", StringComparison.InvariantCultureIgnoreCase) ||
           atRule.StartsWith("@scope", StringComparison.InvariantCultureIgnoreCase) ||
           atRule.StartsWith("@supports", StringComparison.InvariantCultureIgnoreCase) ||
           atRule.StartsWith("@page", StringComparison.InvariantCultureIgnoreCase) ||
           atRule.StartsWith("@keyframes", StringComparison.InvariantCultureIgnoreCase) ||
           atRule.StartsWith("@counter-style", StringComparison.InvariantCultureIgnoreCase) ||
           atRule.StartsWith("@layer", StringComparison.InvariantCultureIgnoreCase) ||
           atRule.StartsWith("@container", StringComparison.InvariantCultureIgnoreCase);

    string BuildCssSelector(IList<string[]> selectors)
    {
        int flatCount = selectors.Count;

        if (flatCount == 0)
        {
            return "";
        }
        if (flatCount == 1)
        {
            return this.BuildCssSelector(selectors[0], Array.Empty<string>());
        }
        else
        {
            string carry = this.BuildCssSelector(selectors[0], Array.Empty<string>());
            for (int i = 1; i < flatCount; i++)
            {
                string[] current = selectors[i];
                if (current.Length == 0) continue;
                carry = this.BuildCssSelector(current, Helper.SafeSplit(carry, ','));
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
                string prepared = Helper.PrepareSelectorUnit(cSelector, this.Context.Options.KeepNestingSpace, this.Context.Options.Pretty);
                sb.Append(prepared);
                sb.Append(',');
                if (this.Context.Options.Pretty)
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
                    if (!this.Context.Options.KeepNestingSpace)
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

                s = Helper.PrepareSelectorUnit(s, this.Context.Options.KeepNestingSpace, this.Context.Options.Pretty);
                sb.Append(s);
                sb.Append(',');
                if (this.Context.Options.Pretty)
                    sb.Append(' ');
            }
        }

    finish:
        if (sb.Length > 0) sb.Length--;
        if (sb.Length > 0 && this.Context.Options.Pretty) sb.Length--;
        return sb.ToString();
    }

    public void Merge(CssStylesheet stylesheet, CascadiumOptions options)
    {
        if (options.Merge.HasFlag(MergeOption.Selectors))
        {
            List<CssRule> newRules = new List<CssRule>();

            foreach (CssRule rule in stylesheet._rules)
            {
                if (IsNotEligibleToSelectorMerge(rule.Selector))
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
            List<CssRule> newRules = new List<CssRule>();

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
