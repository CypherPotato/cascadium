using System.Text;

namespace Cascadium;

/// <summary>
/// Provides a CSS compiler that compiles higher-level code with single-line comments, nesting into a legacy CSS file.
/// </summary>
public sealed partial class CascadiumCompiler
{
    internal static char[] combinators = new[] { '>', '~', '+' };

    private CascadiumCompiler() { }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The byte array including the top module CSS code.</param>
    /// <param name="encoder">The encoder which will be used to decode the CSS output.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified UTF-8 CSS.</returns>
    public static string Compile(ReadOnlySpan<byte> css, Encoding encoder, CascadiumOptions? options = null)
    {
        return Compile(encoder.GetString(css), options);
    }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The top module CSS code.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified CSS.</returns>
    public static string Compile(string css, CascadiumOptions? options = null)
    {
        var context = new Compiler.CompilerContext()
        {
            Options = options
        };
        string prepared = context.Preparers.PrepareCssInput(css);
        context.Parser.ParseCss(prepared);
        return context.Exporter.Export().Trim();
    }
}