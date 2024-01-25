using Cascadium.Compiler;
using Cascadium.Entity;
using Cascadium.Extensions;
using Cascadium.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium;

/// <summary>
/// Provides a CSS compiler that compiles higher-level code with single-line comments, nesting into a legacy CSS file.
/// </summary>
public sealed partial class CascadiumCompiler
{
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
        CascadiumOptions _options = options ?? new CascadiumOptions();

        //// strip comments and trim the input
        string sanitizedInput = Sanitizer.SanitizeInput(xcss);

        //// tokenizes the sanitized input, which is the lexical analyzer
        //// and convert the code into tokens
        TokenCollection resultTokens = new Tokenizer(sanitizedInput).Tokenize();

        //// parses the produced tokens and produce an nested stylesheet from
        //// the token. this also applies the semantic and syntax checking
        NestedStylesheet nestedStylesheet = Parser.ParseSpreadsheet(resultTokens);

        //// flatten the stylesheet, which removes the nesting and places all
        //// rules in the top level of the stylesheet.
        FlatStylesheet flattenStylesheet = Flattener.FlattenStylesheet(nestedStylesheet);

        //// build the css body, assembling the flat rules into an valid
        //// css string
        CssStylesheet css = new Assembler(_options).AssemblyCss(flattenStylesheet, _options);

        //// apply cascadium extensions
        if (_options.UseVarShortcut) ValueHandler.TransformVarShortcuts(css);
        if (_options.AtRulesRewrites.Count > 0) MediaRewriter.ApplyRewrites(css, _options);
        if (_options.Converters.Count > 0) Converter.ConvertAll(css, _options);

        //// export the css into an string
        return css.Export(_options);
    }
}