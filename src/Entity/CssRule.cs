using System.Collections.Generic;

namespace Cascadium.Entity;

/// <summary>
/// Represents an CSS rule.
/// </summary>
public class CssRule
{
    internal Dictionary<string, string> _declarations { get; set; } = new Dictionary<string, string>();
    internal int _order = 0;

    /// <summary>
    /// Gets the rule selector.
    /// </summary>
    public string Selector { get; internal set; } = "";

    /// <summary>
    /// Gets the declarations defined in this <see cref="CssRule"/>.
    /// </summary>
    public IDictionary<string, string> Declarations { get => _declarations; }

    /// <summary>
    /// Gets the hash code for this <see cref="CssRule"/>.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        int n = 0, j = 1;

        // the property order should impact on the hash code
        // so
        // foo: bar
        // bar: foo
        //
        // is different than
        //
        // bar: foo
        // foo: bar

        foreach (var kp in _declarations)
        {
            n += (kp.Key.GetHashCode() + kp.Value.GetHashCode()) / 2;
            n *= j;
            j++;
        }
        return n / _declarations.Count;
    }

    /// <summary>
    /// Determines if the specified objects are <see cref="CssRule"/> and are equals.
    /// </summary>
    /// <param name="obj">The another <see cref="CssRule"/>.</param>
    /// <returns>A boolean indicating if this object is equals to the other one.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is CssRule r)
        {
            return this.GetHashCode() == r.GetHashCode();
        }
        return false;
    }
}
