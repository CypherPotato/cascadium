using System.Collections.Generic;

namespace Cascadium.Entity;

interface IRuleContainer
{
    public List<NestedRule> Rules { get; set; }
}
