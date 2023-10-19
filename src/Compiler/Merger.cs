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

    public void Merge(bool mergeRules, bool mergeAtRules)
    {
        if (mergeRules)
        {
            List<CssRule> newRules = new List<CssRule>();

            foreach (CssRule rule in this.Context.Rules)
            {
                CssRule? existingRule = newRules
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
                    }
                }
            }

            this.Context.Rules = newRules;
        }

        if (mergeAtRules)
        {
            List<CompilerContext> stylesheets = new List<CompilerContext>();

            foreach (CompilerContext css in this.Context.Stylesheets)
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
                css.Merger.Merge(mergeRules, mergeAtRules);
            }

            this.Context.Stylesheets = stylesheets;
        }
    }
}
