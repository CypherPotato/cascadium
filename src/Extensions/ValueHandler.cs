using Cascadium.Entity;
using Cascadium.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Extensions;

internal class ValueHandler
{
    public static void TransformVarShortcuts(CssStylesheet css)
    {
        foreach (var rule in css.Rules)
        {
            foreach(string key in rule.Declarations.Keys)
            {
                rule.Declarations[key] = ApplyVarShortcuts(rule.Declarations[key]);
            }
        }
        foreach (var subcss in css.Stylesheets)
        {
            TransformVarShortcuts(subcss);
        }
    }

    static string ApplyVarShortcuts(string value)
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
                if (c == '-' && b == '-' && output.Length >= 2 && !output.ToString().EndsWith("var(--"))
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
