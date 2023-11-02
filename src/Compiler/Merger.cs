using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;

internal class Merger : CompilerModule
{
    public Merger(CompilerContext context) : base(context)
    {
    }

    public void Merge(bool mergeSelectors, bool mergeAtRules, bool mergeDeclarations)
    {
        if (mergeSelectors)
        {
            List<Rule> newRules = new List<Rule>();

            foreach (Rule rule in this.Context.Rules)
            {
                Rule? existingRule = newRules
                    .FirstOrDefault(r => Context.Preparers.IsSelectorsEqual(r.Selector, rule.Selector));

                if (existingRule == null)
                {
                    newRules.Add(rule);
                }
                else
                {
                    foreach (var prop in rule.Properties)
                    {
                        existingRule.Properties[prop.Key] = prop.Value;

                        if (this.Context.Options?.MergeOrderPriority == MergeOrderPriority.PreserveLast)
                        {
                            if (rule.Order > existingRule.Order)
                                existingRule.Order = rule.Order;
                        }
                    }
                }
            }

            this.Context.Rules = newRules;
        }

        if (mergeAtRules)
        {
            List<CompilerContext> stylesheets = new List<CompilerContext>();

            foreach (CompilerContext css in this.Context.Childrens)
            {
                if (css.AtRule == null) continue;

                CompilerContext? existingCss = stylesheets
                    .FirstOrDefault(r => Context.Utils.RemoveSpaces(r.AtRule ?? "") == Context.Utils.RemoveSpaces(css.AtRule));

                if (existingCss == null)
                {
                    stylesheets.Add(css);
                }
                else
                {
                    existingCss.Rules.AddRange(css.Rules);
                }
            }

            foreach (CompilerContext css in stylesheets)
            {
                css.Merger.Merge(mergeSelectors, mergeAtRules, mergeDeclarations);
            }

            this.Context.Childrens = stylesheets;
        }

        if (mergeDeclarations)
        {
            // merge top-level only
            List<Rule> newRules = new List<Rule>();

            foreach (Rule rule in this.Context.Rules)
            {
                Rule? existingRule = newRules
                    .FirstOrDefault(r => r.GetPropertiesHashCode() == rule.GetPropertiesHashCode());

                if (existingRule == null)
                {
                    newRules.Add(rule);
                }
                else
                {
                    existingRule.Selector = Context.Preparers.CombineSelectors(existingRule.Selector, rule.Selector);
                }
            }

            this.Context.Rules = newRules;
        }
    }
}
