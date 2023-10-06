using SimpleCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CssTests.Tests;

[CssTest]
internal class SelectorTest3 : SimpleCssTest
{
    public override String Name => "Selector test 3";

    public override String Description => "Tests with the CSS selector syntax";

    public override String Input =>
        """
        div[content="ax!@#$%&'"] {
            foo: bar;
        }
        """;

    public override CSSCompilerOptions Options { get; set; } = new CSSCompilerOptions();
}
