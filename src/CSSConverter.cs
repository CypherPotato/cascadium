using System.Collections.Specialized;
using System.Text;

namespace SimpleCSS;

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
        if (value == null) return Array.Empty<string>();
        List<string> output = new List<string>();
        StringBuilder mounting = new StringBuilder();
        bool inSingleString = false;
        bool inDoubleString = false;
        int expressionIndex = 0;

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            char b = i > 0 ? value[i - 1] : '\0';
            mounting.Append(c);

            if (c == '\'' && b != '\\')
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\')
            {
                inDoubleString = !inDoubleString;
            }
            else if (c == '(' && !(inDoubleString || inSingleString))
            {
                expressionIndex++;
            }
            else if (c == ')' && !(inDoubleString || inSingleString))
            {
                expressionIndex--;
            }

            if ((inDoubleString || inSingleString) == false && expressionIndex == 0)
            {
                if (c == ' ')
                {
                    mounting.Length--;
                    output.Add(mounting.ToString());
                    mounting.Clear();
                }
            }
        }

        if (mounting.Length > 0)
            output.Add(mounting.ToString());

        return output.ToArray();
    }
}
