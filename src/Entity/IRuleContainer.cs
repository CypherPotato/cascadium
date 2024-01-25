using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Entity;

interface IRuleContainer
{
    public List<NestedRule> Rules { get; set; }
}
