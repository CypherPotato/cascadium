namespace CypherPotato;

/// <summary>
/// Specifies compilation properties for CSS generation in <see cref="SimpleCSSCompiler"/>.
/// </summary>
public class CSSCompilerOptions
{
    /// <summary>
    /// Gets or sets whether the CSS output should be exported indented and well-formatted.
    /// </summary>
    public bool Pretty { get; set; } = false;
}
