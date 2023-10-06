using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleCSS.Converters;

public class StaticCSSConverter : CSSConverter
{
    public IDictionary<string, string> Output { get; set; } = new Dictionary<string, string>();
    public string MatchProperty { get; set; }
    public int? ArgumentCount { get; set; }

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

    public override void Convert(String? value, NameValueCollection outputDeclarations)
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
}