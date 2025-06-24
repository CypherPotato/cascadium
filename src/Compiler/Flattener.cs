using Cascadium.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cascadium.Compiler;

internal class Flattener
{
    public static FlatStylesheet FlattenStylesheet(NestedStylesheet nestedStylesheet)
    {
        FlatStylesheet output = new FlatStylesheet();

        static void CreateRules(FlatStylesheet output, NestedRule rule, IEnumerable<NestedRule> parents)
        {
            List<string[]> selectors = [];

            foreach (NestedRule parent in parents)
            {
                if (parent.Selectors.Count == 0)
                    continue;

                if (AtRule.IsNotParentInherited(parent.Selectors[0]))
                    selectors.Clear();
                selectors.Add(parent.Selectors.ToArray());
            }

            selectors.Add(rule.Selectors.ToArray());

            var frule = new FlatRule()
            {
                Selectors = selectors,
                Declarations = rule.Declarations
            };
            output.Rules.Add(frule);

            foreach (NestedRule r in rule.Rules)
            {
                CreateRules(output, r, parents.Concat([rule]));
            }
        }

        static bool CanFlattenParentSelectors(NestedRule rule)
        {
            return rule.Selectors.Count > 0 && !AtRule.IsNotParentInherited(rule.Selectors[0]);
        }

        foreach (NestedRule r in nestedStylesheet.Rules)
        {
            CreateRules(output, r, Array.Empty<NestedRule>());
        }

        output.Statements.AddRange(nestedStylesheet.Statements);

        return output;
    }
}
