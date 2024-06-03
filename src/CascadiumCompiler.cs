using Cascadium.Compiler;
using Cascadium.Entity;
using Cascadium.Extensions;
using Cascadium.Object;
using System;
using System.Text;

namespace Cascadium;

/// <summary>
/// Provides a compiler that compiles Cascadium code into an legacy CSS file.
/// </summary>
public static class CascadiumCompiler
{
    /// <summary>
    /// Compiles an Cascadium stylesheet to legacy CSS.
    /// </summary>
    /// <param name="xcss">The Cascadium source code.</param>
    /// <param name="options">Optional. Options and parameters to the compiler.</param>
    /// <returns>The compiled and minified CSS.</returns>
    public static CssStylesheet Parse(string xcss, CascadiumOptions? options = null)
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
        css.Options = _options;

        //// apply cascadium extensions
        if (_options.UseVarShortcut) ValueHandler.TransformVarShortcuts(css);
        if (_options.AtRulesRewrites.Count > 0) MediaRewriter.ApplyRewrites(css, _options);
        if (_options.Converters.Count > 0) Converter.ConvertAll(css, _options);

        return css;
    }

    /// <summary>
    /// Compiles an Cascadium stylesheet to legacy CSS.
    /// </summary>
    /// <param name="xcss">The Cascadium source code.</param>
    /// <param name="options">Optional. Options and parameters to the compiler.</param>
    /// <returns>The compiled and minified CSS.</returns>
    public static string Compile(string xcss, CascadiumOptions? options = null)
    {
        CascadiumOptions _options = options ?? new CascadiumOptions();
        return Parse(xcss, _options).Export();
    }

    /// <summary>
    /// Compiles an Cascadium stylesheet to legacy CSS.
    /// </summary>
    /// <param name="xcss">The byte array including the Cascadium source code.</param>
    /// <param name="options">Optional. Options and parameters to the compiler.</param>
    /// <returns>The compiled UTF-8 CSS.</returns>
    public static string Compile(ReadOnlySpan<byte> xcss, CascadiumOptions? options = null)
    {
        return Compile(xcss, Encoding.UTF8, options);
    }

    /// <summary>
    /// Compiles an Cascadium stylesheet to legacy CSS.
    /// </summary>
    /// <param name="xcss">The byte array including the Cascadium source code.</param>
    /// <param name="encoder">The encoder which will be used to decode the CSS output.</param>
    /// <param name="options">Optional. Options and parameters to the compiler.</param>
    /// <returns>The compiled CSS.</returns>
    public static string Compile(ReadOnlySpan<byte> xcss, Encoding encoder, CascadiumOptions? options = null)
    {
        return Compile(encoder.GetString(xcss), options);
    }
}