using Cascadium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CssTests.Tests;

[CssTest]
internal class SelectorTest2 : SimpleCssTest
{
    public override String Name => "Selector test 2";

    public override String Description => "Tests with the CSS selector syntax";

    public override String Input =>
        """
        div.foo,
        div.bar {
            ay: by;

            & :hover,
            & :active {
                az: bz;
            }
        }

        div:is(.foo, .bar),
        span:is(.baz) {
            color: blue;

            & :is(:focus, :not(baz, boz)) {
                aka: aba;
            }
        }
        """;

    public override CascadiumOptions Options { get; set; } = new CascadiumOptions();
}
