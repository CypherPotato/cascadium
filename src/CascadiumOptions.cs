using Cascadium.Converters;
using System;
using System.Collections.Generic;
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
    public MergeOption Merge { get; set; } = MergeOption.None;

    /// <summary>
    /// Gets or sets how the merge will prioritize the order of the rules as it finds them.
    /// </summary>
    public MergeOrderPriority MergeOrderPriority { get; set; } = MergeOrderPriority.PreserveLast;
}


/// <summary>
/// Represents the order priority used at an <see cref="CascadiumOptions"/>, only appliable to <see cref="MergeOption.Selectors"/>.
/// </summary>
public enum MergeOrderPriority
{
    /// <summary>
    /// Specifies that the first encountered rule position should be preserved.
    /// </summary>
    PreserveFirst,

    /// <summary>
    /// Specifies that the last encountered rule position should be preserved.
    /// </summary>
    PreserveLast
}

/// <summary>
/// Represents the merge option which will be used at an <see cref="CascadiumOptions"/>.
/// </summary>
[Flags]
public enum MergeOption
{
    /// <summary>
    /// Specifies that no merge should be made.
    /// </summary>
    None = 1 << 1,

    /// <summary>
    /// Specifies that CSS selectors should be merged.
    /// </summary>
    Selectors = 1 << 2,

    /// <summary>
    /// Specifies that at-rules identifier and rule should be merged.
    /// </summary>
    AtRules = 1 << 3,

    /// <summary>
    /// Specifies that exact declarations with different selectors should be merged.
    /// </summary>
    Declarations = 1 << 4,

    /// <summary>
    /// Specifies that all merge options must be evaluated.
    /// </summary>
    All = Selectors | AtRules | Declarations
}