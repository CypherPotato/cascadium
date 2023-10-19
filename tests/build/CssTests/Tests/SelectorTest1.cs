using Cascadium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CssTests.Tests;

[CssTest]
internal class SelectorTest1 : SimpleCssTest
{
    public override String Name => "Selector test 1";

    public override String Description => "Tests with the CSS selector syntax";

    public override String Input =>
        """
        // ugly, but valid selector
        div.foo:not(.bar)[active] > input[type="text"] {
            prop1: val1;
            prop2: val2
        }

        // spaces test
        div[a][b] [c] [d]   [e][f] {
            foo: bar;
        }
        """;

    public override CascadiumOptions Options { get; set; } = new CascadiumOptions();
}
