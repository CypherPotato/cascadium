using System.Collections.Generic;

namespace Cascadium.Entity;

internal class NestedStylesheet : IRuleContainer
{
    public List<string> Statements { get; set; } = new List<string>();
    public List<NestedRule> Rules { get; set; } = new List<NestedRule>();
}
