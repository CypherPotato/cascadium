using Cascadium.Compiler;
using System.Collections.Specialized;
using System.Text;

namespace Cascadium.Converters;

/// <summary>
/// Provides an utility for converting CSS properties and/or values.
/// </summary>
public abstract class CSSConverter
{
    /// <summary>
    /// Indicates if the declaration should be converted.
    /// </summary>
    /// <param name="propertyName">The CSS property name.</param>
    /// <param name="value">The CSS raw value.</param>
    public abstract bool CanConvert(string propertyName, string value);

    /// <summary>
    /// Converts the declaration values.
    /// </summary>
    /// <param name="value">The input raw value.</param>
    /// <param name="outputDeclarations">Defines new declarations which will be written to.</param>
    public abstract void Convert(string? value, NameValueCollection outputDeclarations);

    /// <summary>
    /// Splits the CSS value into safe padded values.
    /// </summary>
    /// <param name="value">The raw CSS value.</param>
    public string[] SafeSplit(string? value)
    {
        return Split.StSafeSplit(value, ' ');
    }
}
