using Cascadium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CssTests.Tests;

[CssTest]
internal class NestingTest2 : SimpleCssTest
{
    public override string Name => "Nesting test 2";

    public override string Description => "Nesting test with @media rules";

    public override string Input =>
        """
        div {
            foo: bar;
            baz: apx 123 daz;

            // should be interpreted as an @media
            @media (max-width: 123) {
                baz: 512 kox;
            }

            // should be interpreted as an sub-rule
            > @media {
                include: this;
            }
        }
        """;

    public override CascadiumOptions Options { get; set; } = new CascadiumOptions()
    {

    };
}
