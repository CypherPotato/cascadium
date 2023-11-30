using System;
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
    /// <param name="xcss">The byte array including the top module CSS code.</param>
    /// <param name="encoder">The encoder which will be used to decode the CSS output.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified UTF-8 CSS.</returns>
    public static string Compile(ReadOnlySpan<byte> xcss, Encoding encoder, CascadiumOptions? options = null)
    {
        return Compile(encoder.GetString(xcss), options);
    }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="xcss">The top module CSS code.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified CSS.</returns>
    public static string Compile(string xcss, CascadiumOptions? options = null)
    {
        var context = new Compiler.CompilerContext()
        {
            Options = options
        };
        string prepared = context.Preparers.PrepareCssInput(xcss);
        context.Parser.ParseXCss(prepared);
        return context.Exporter.Export().Trim();
    }
}