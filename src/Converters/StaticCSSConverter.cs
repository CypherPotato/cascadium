using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cascadium.Converters;

/// <summary>
/// Provides an static <see cref="CSSConverter"/> which converts contents
/// matching the property name and argument count.
/// </summary>
public class StaticCSSConverter : CSSConverter
{
    /// <summary>
    /// Gets or sets the output declarations for the CSS output.
    /// </summary>
    public IDictionary<string, string> Output { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the property name which will be matched.
    /// </summary>
    public string? MatchProperty { get; set; }

    /// <summary>
    /// Gets or sets the argument count that should match with the property
    /// value.
    /// </summary>
    public int? ArgumentCount { get; set; }

    /// <inheritdoc/>
    public override Boolean CanConvert(String propertyName, String value)
    {
        if (string.Compare(propertyName, MatchProperty, true) == 0)
        {
            if (ArgumentCount != null)
            {
                if (ArgumentCount == SafeSplit(value).Length)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public override void Convert(String? value, NameValueCollection outputDeclarations)
    {
        if (ArgumentCount != null)
        {
            string[] arguments = SafeSplit(value);
            foreach (KeyValuePair<string, string> pair in Output)
            {
                string newValue = pair.Value;

                for (int i = 0; i < arguments.Length; i++)
                {
                    newValue = newValue.Replace("$" + (i + 1), arguments[i]);
                }

                outputDeclarations.Add(pair.Key, newValue);
            }
        }
        else
        {
            foreach (KeyValuePair<string, string> pair in Output)
            {
                string newValue = pair.Value.Replace("$*", value);
                outputDeclarations.Add(pair.Key, newValue);
            }
        }
    }
}