using Cascadium.Entity;
using Cascadium.Object;
using System.Text;

namespace Cascadium.Extensions;

internal class ValueHandler
{
    public static void TransformVarShortcuts(CssStylesheet css)
    {
        foreach (var rule in css._rules)
        {
            foreach (string key in rule._declarations.Keys)
            {
                rule._declarations[key] = ApplyVarShortcuts(rule._declarations[key], css);
            }
        }
        foreach (var subcss in css._stylesheets)
        {
            TransformVarShortcuts(subcss);
        }
    }

    static string ApplyVarShortcuts(string value, CssStylesheet stylesheet)
    {
        StringBuilder output = new StringBuilder();
        char[] chars = value.ToCharArray();
        bool inSingleString = false;
        bool inDoubleString = false;
        bool isParsingVarname = false;

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            char b = i > 0 ? chars[i - 1] : '\0';
            char n = i < chars.Length - 1 ? chars[i + 1] : '\0';

            output.Append(c);

            if (c == '\'' && b != '\\' && !inDoubleString)
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\' && !inSingleString)
            {
                inDoubleString = !inDoubleString;
            }

            if ((inSingleString || inDoubleString) == false)
            {
                string tmpOut = output.ToString();
                if (c == '-' && b == '-' && n != '-' && output.Length >= 2 && !tmpOut.EndsWith("var(--") && !tmpOut.EndsWith("---"))
                {
                    isParsingVarname = true;
                    output.Length -= 2;
                    output.Append("var(--");
                }
                else if (!Token.IsIdentifierChr(c) && isParsingVarname)
                {
                    output.Length--;
                    output.Append(')');
                    output.Append(c);
                    isParsingVarname = false;
                }
            }
        }

        if (isParsingVarname)
            output.Append(')');

        return output.ToString();
    }
}
