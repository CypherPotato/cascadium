using System.Text;

namespace SimpleCSS;

/// <summary>
/// Provides a CSS compiler that compiles higher-level code with single-line comments, nesting into a legacy CSS file.
/// </summary>
public sealed partial class SimpleCSSCompiler
{
    private string? AtRule { get; set; }
    private List<CssRule> Rules { get; set; } = new List<CssRule>();
    private List<SimpleCSSCompiler> Stylesheets { get; set; } = new List<SimpleCSSCompiler>();
    private List<string> Declarations { get; set; } = new List<string>();

    private CSSCompilerOptions? Options { get; set; }

    private SimpleCSSCompiler() { }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The byte array including the top module CSS code.</param>
    /// <param name="encoder">The encoder which will be used to decode the CSS output.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified UTF-8 CSS.</returns>
    public static string Compile(ReadOnlySpan<byte> css, Encoding encoder, CSSCompilerOptions? options = null)
    {
        return Compile(encoder.GetString(css), options);
    }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The top module CSS code.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified CSS.</returns>
    public static string Compile(string css, CSSCompilerOptions? options = null)
    {
        string prepared = PrepareCssInput(css);
        SimpleCSSCompiler s = new SimpleCSSCompiler() { Options = options };
        s.ParseCss(prepared);
        return s.Export().Trim();
    }


}