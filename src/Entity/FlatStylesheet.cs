using System.Collections.Generic;

namespace Cascadium.Entity;

class FlatStylesheet
{
    public List<string> Statements { get; set; } = new();
    public List<FlatRule> Rules { get; set; } = new();
}
