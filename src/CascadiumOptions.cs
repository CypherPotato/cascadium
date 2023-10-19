using Cascadium.Converters;
using System.Collections.Specialized;

namespace Cascadium;

/// <summary>
/// Specifies compilation properties for CSS generation in <see cref="CascadiumCompiler"/>.
/// </summary>
public class CascadiumOptions
{
    /// <summary>
    /// Gets or sets whether the space between the &amp; operator and the selector
    /// should be keept.
    /// </summary>
    public bool KeepNestingSpace { get; set; } = false;

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

    /// <summary>
    /// Gets or sets an list of @-rules which will be replaced by the specified values.
    /// </summary>
    public NameValueCollection AtRulesRewrites { get; set; } = new NameValueCollection();

    /// <summary>
    /// Gets or sets whether equals rules and at-rules declarations should be merged or not.
    /// </summary>
    public bool Merge { get; set; } = false;
}