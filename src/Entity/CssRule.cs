using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Entity;

internal class CssRule
{
    public string Selector { get; set; } = "";
    public Dictionary<string, string> Declarations { get; set; } = new Dictionary<string, string>();
    public int Order { get; set; } = 0;

    public override int GetHashCode()
    {
        int n = 0, j = 1;

        // the property order should impact on the hash code
        // so
        // foo: bar
        // bar: foo
        //
        // is different than
        //
        // bar: foo
        // foo: bar

        foreach (var kp in Declarations)
        {
            n += (kp.Key.GetHashCode() + kp.Value.GetHashCode()) / 2;
            n *= j;
            j++;
        }
        return n / Declarations.Count;
    }
}
