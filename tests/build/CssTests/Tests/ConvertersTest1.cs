using Cascadium;
using Cascadium.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CssTests.Tests;

[CssTest]
internal class ConvertersTest1 : SimpleCssTest
{
    public override string Name => "Static converter test 1";

    public override string Description => "Testing nesting module with converters";

    public override string Input =>
        """
        div.a2 {
            $my-border: gainsboro red;
        }

        div.a3 {
            $my-border: arg1 arg2 "foo bar";
        }
        """;

    public override CascadiumOptions Options { get; set; } = new CascadiumOptions()
    {
        UseVarShortcut = true,
        Converters = new List<Cascadium.Converters.CSSConverter>()
        {
            new StaticCSSConverter()
            {
                ArgumentCount = 2,
                MatchProperty = "$my-border",
                Output = new Dictionary<string, string>()
                {
                    { "border", "1px solid $1" },
                    { "outline", "1px solid $2" }
                }
            },
            new StaticCSSConverter()
            {
                ArgumentCount = 3,
                MatchProperty = "$my-border",
                Output = new Dictionary<string, string>()
                {
                    { "border", "1px solid $1" },
                    { "outline", "1px solid $2" },
                    { "background", "url($3) cover" }
                }
            },
        }
    };
}
