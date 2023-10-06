using SimpleCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CssTests.Tests;

[CssTest]
internal class NestingTest1 : SimpleCssTest
{
    public override string Name => "Nesting test 1";

    public override string Description => "Testing nesting module with some complex nestings";

    public override string Input =>
        """
        nav {
          ul {
            margin: 0;
            padding: 0;
            list-style: none;
          }

          li { display: inline-block; }

          a {
            display: block;
            padding: 6px 12px;
            text-decoration: none;
          }
        }
        """;

    public override CSSCompilerOptions Options { get; set; } = new CSSCompilerOptions()
    {
        UseVarShortcut = true,
    };
}
