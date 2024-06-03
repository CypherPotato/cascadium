using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Cascadium.Entity;

class FlatRule
{
    public List<string[]> Selectors { get; set; } = new List<string[]>();
    public Dictionary<string, string> Declarations { get; set; } = new Dictionary<string, string>();

    public override string ToString() => $"{string.Join(" ", Selectors.Select(s => string.Join(",", s)))} Declarations={Declarations.Count}";

    public bool IsRuleAtRule()
    {
        return Selectors.Count == 1 && Selectors[0].Length == 1 && Selectors[0][0].StartsWith('@');
    }

    public string? PopAtRule()
    {
        string? s = null;
        foreach (string[] group in Selectors)
        {
            foreach (string selector in group)
            {
                if (selector.StartsWith('@'))
                {
                    s = selector;
                }
            }
        }

        Selectors = Selectors.Select(group => group.Where(ss => ss != s).ToArray()).ToList();

        return s;
    }
}
