using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Entity;

class NestedRule : IRuleContainer
{
    public List<string> Selectors { get; set; } = new List<string>();
    public Dictionary<string, string> Declarations { get; set; } = new Dictionary<string, string>();
    public List<NestedRule> Rules { get; set; } = new List<NestedRule>();

    public override string ToString() => $"{string.Join(",", Selectors)} Declarations={Declarations.Count} Rules={Rules.Count}";
}
