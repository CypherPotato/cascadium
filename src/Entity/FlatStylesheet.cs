using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Entity;

class FlatStylesheet
{
    public List<string> Statements { get; set; } = new();
    public List<FlatRule> Rules { get; set; } = new();
}
