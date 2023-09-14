namespace SimpleCSS;

/// <summary>
/// Specifies compilation properties for CSS generation in <see cref="SimpleCSSCompiler"/>.
/// </summary>
public class CSSCompilerOptions
{
    /// <summary>
    /// Gets or sets whether the CSS output should be exported indented and well-formatted.
    /// </summary>
    public bool Pretty { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the CSS compiler should automatically rewrite <code>--variable</code> to <code>var(--variable)</code>
    /// </summary>
    public bool UseVarShortcut { get; set; } = false;

    /// <summary>
    /// Gets or sets an list of <see cref="CSSConverter"/> which will be used in this CSS Compiler.
    /// </summary>
    public List<CSSConverter> Converters { get; set; } = new List<CSSConverter>();
}
