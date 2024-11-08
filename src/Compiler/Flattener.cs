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
            List<string[]> selectors = new List<string[]>();

            foreach (NestedRule parent in parents)
            {
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

        foreach (NestedRule r in nestedStylesheet.Rules)
        {
            CreateRules(output, r, Array.Empty<NestedRule>());
        }

        output.Statements.AddRange(nestedStylesheet.Statements);

        return output;
    }
}
