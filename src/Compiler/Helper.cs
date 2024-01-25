using Cascadium.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;

internal static class Helper
{
    public static string[] SafeSplit(string? value, char op)
    {
        if (value == null) return Array.Empty<string>();
        List<string> output = new List<string>();
        StringBuilder mounting = new StringBuilder();
        bool inSingleString = false;
        bool inDoubleString = false;
        int expressionIndex = 0, groupIndex = 0;

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
            else if (c == '[' && !(inDoubleString || inSingleString))
            {
                groupIndex++;
            }
            else if (c == ']' && !(inDoubleString || inSingleString))
            {
                groupIndex--;
            }

            if ((inDoubleString || inSingleString) == false && expressionIndex == 0 && groupIndex == 0)
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

    public static bool InvariantCompare(string? a, string? b)
    {
        return string.Compare(RemoveSpaces(a ?? ""), RemoveSpaces(b ?? "")) == 0;
    }

    public static string RemoveSpaces(string s)
        => new String(s.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());

    public static bool IsSelectorsEqual(string? a, string? b)
    {
        if (a == null || b == null) return a == b;
        return PrepareSelectorUnit(a) == PrepareSelectorUnit(b);
    }

    public static string PrepareSelectorUnit(string s, bool keepNestingSpace = false, bool pretty = false)
    {
        StringBuilder output = new StringBuilder();
        char[] chars;

        bool ignoreTrailingSpace = keepNestingSpace == false;

        if (ignoreTrailingSpace)
        {
            chars = s.Trim().ToCharArray();
        }
        else
        {
            chars = s.TrimEnd().ToCharArray();
        }

        bool inSingleString = false;
        bool inDoubleString = false;
        char lastCombinator = '\0';

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            char b = i > 0 ? chars[i - 1] : '\0';

            if (c == '\'' && b != '\\' && !inDoubleString)
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\' && !inSingleString)
            {
                inDoubleString = !inDoubleString;
            }

            if (inDoubleString || inSingleString)
            {
                output.Append(c);
                continue;
            }

            if (i == 0 && ignoreTrailingSpace && char.IsWhiteSpace(c))
            {
                output.Append(c);
            }

            if (Token.Sel_Combinators.Contains(c) || char.IsWhiteSpace(c))
            {
                if (char.IsWhiteSpace(lastCombinator) || lastCombinator == '\0')
                    lastCombinator = c;
            }
            else
            {
                bool prettySpace = pretty == true && !char.IsWhiteSpace(lastCombinator) && lastCombinator != '\0';
                if (prettySpace) output.Append(' ');

                if (lastCombinator != '\0')
                    output.Append(lastCombinator);

                if (prettySpace) output.Append(' ');
                output.Append(c);
                lastCombinator = '\0';
            }
        }

        return output.ToString();
    }

    public static string CombineSelectors(string a, string b, bool pretty)
    {
        string[] A = SafeSplit(a, ',').Select(a => a.Trim()).ToArray();
        string[] B = SafeSplit(b, ',').Select(b => b.Trim()).ToArray();

        List<string> output = new List<string>();

        foreach (string n in A.Concat(B))
        {
            if (!output.Contains(n))
                output.Add(n);
        }

        if (pretty == true)
        {
            return String.Join(", ", output.OrderByDescending(o => o.Length));
        }
        else
        {
            return String.Join(',', output);
        }
    }
}
