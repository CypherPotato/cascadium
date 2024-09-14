using System;
using System.Collections.Generic;

namespace Cascadium.Entity;

class NestedRule : IRuleContainer
{
    public List<string> Selectors { get; set; } = new List<string>();
    public Dictionary<string, string> Declarations { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public List<NestedRule> Rules { get; set; } = new List<NestedRule>();

    public override string ToString() => $"{string.Join(",", this.Selectors)} Declarations={this.Declarations.Count} Rules={this.Rules.Count}";
}
