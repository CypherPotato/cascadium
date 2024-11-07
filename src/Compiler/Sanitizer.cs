using System;
using System.Text;

namespace Cascadium.Compiler;

class Sanitizer
{
    public static string SanitizeInput(string input)
    {
        StringBuilder output = new StringBuilder(input.Length);
        ReadOnlySpan<char> inputChars = input;

        bool inSingleString = false;
        bool inDoubleString = false;
        bool inSinglelineComment = false;
        bool inMultilineComment = false;

        var inString = () => inSingleString || inDoubleString;
        var inComment = () => inMultilineComment || inSinglelineComment;

        int len = inputChars.Length;
        for (int i = 0; i < len; i++)
        {
            char current = inputChars[i];
            char before = i > 0 ? inputChars[i - 1] : '\0';
            char next = len - 1 > i ? inputChars[i + 1] : '\0';

            if (current == '\'' && before != '\\' && !inDoubleString && !inComment())
            {
                inSingleString = !inSingleString;
            }
            else if (current == '"' && before != '\\' && !inSingleString && !inComment())
            {
                inDoubleString = !inDoubleString;
            }
            else if (current == '*' && before == '/' && !inString() && !inSinglelineComment)
            {
                inMultilineComment = true;
                if (output.Length > 0) output.Length--;
                continue;
            }
            else if (current == '/' && before == '*' && !inString() && !inSinglelineComment)
            {
                inMultilineComment = false;
                continue;
            }
            else if (current == '/' && before == '/' && next != '*' && !inString() && !inMultilineComment)
            {
                inSinglelineComment = true;
                if (output.Length > 0) output.Length--;
                continue;
            }
            else if ((current == '\n' || current == '\r') && !inString() && inSinglelineComment)
            {
                inSinglelineComment = false;
            }

            if (!inComment())
            {
                output.Append(current);
            }
        }

        return output.ToString();
    }
}
