using SimpleCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CssTests.Tests;

[CssTest]
internal class AtRulesTest1 : SimpleCssTest
{
    public override string Name => "At-Rule test 1";

    public override string Description => "Testing nesting module with at rules";

    public override string Input =>
        """
        @import "../mobile-styles.css";

        @font-face {
            font: aaa;
            src: bbb;
        }

        div {
            display: grid;
            grid-template-columns: repeat(3, 1fr);

            > * {
                
                & :nth-child(1) {
                    background-color: red;
                }

                & :nth-child(2) {
                    background-color: yellow;

                    @media mobile{ background-color: red; }
                }

                & :nth-child(3) {
                    background-color: green;
                }
            }

            @media mobile {
                grid-template-columns: 1fr;
            }
        }
        """;

    public override CSSCompilerOptions Options { get; set; } = new CSSCompilerOptions()
    {
        UseVarShortcut = true,
        AtRulesRewrites = new System.Collections.Specialized.NameValueCollection()
        {
            { "media mobile", "media only screen and (max-width: 712px)" }
        }
    };
}
