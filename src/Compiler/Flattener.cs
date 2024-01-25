using Cascadium.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;

internal class Flattener
{
    public static FlatStylesheet FlattenStylesheet(NestedStylesheet nestedStylesheet)
    {
        FlatStylesheet output = new FlatStylesheet();

        void CreateRules(NestedRule rule, IEnumerable<NestedRule> parents)
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
                CreateRules(r, parents.Concat(new[] { rule }));
            }
        }

        foreach (NestedRule r in nestedStylesheet.Rules)
        {
            CreateRules(r, Array.Empty<NestedRule>());
        }

        output.Statements.AddRange(nestedStylesheet.Statements);

        return output;
    }
}
