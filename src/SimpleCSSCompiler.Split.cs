using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCSS;
public sealed partial class SimpleCSSCompiler
{
    internal static string[] SafeSplit(string? value, char op)
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

            if (c == '\'' && b != '\\' && !inDoubleString)
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\' && !inSingleString)
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
                if (c == op)
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
