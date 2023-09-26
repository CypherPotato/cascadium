using System.Text;

namespace SimpleCSS;

public sealed partial class SimpleCSSCompiler
{
    private static char[] combinators = new[] { '>', '~', '+' };

    static string PrepareCssInput(string input)
    {
        StringBuilder output = new StringBuilder();

        char[] inputChars = input.ToCharArray();

        bool inSingleString = false;
        bool inDoubleString = false;
        bool inSinglelineComment = false;
        bool inMultilineComment = false;

        var inString = () => inSingleString || inDoubleString;
        var inComment = () => inMultilineComment || inSinglelineComment;

        for (int i = 0; i < inputChars.Length; i++)
        {
            char current = inputChars[i];
            char before = i > 0 ? inputChars[i - 1] : '\0';

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
            }
            else if (current == '/' && before == '*' && !inString() && !inSinglelineComment)
            {
                inMultilineComment = false;
                if (output.Length > 0) output.Length--;
                continue;
            }
            else if (current == '/' && before == '/' && !inString() && !inMultilineComment)
            {
                inSinglelineComment = true;
                if (output.Length > 0) output.Length--;
            }
            else if (current == '\n' || current == '\r' && !inString() && inSinglelineComment)
            {
                inSinglelineComment = false;
            }

            if (!inComment())
            {
                output.Append(current);
            }
        }

        return output.ToString().Trim();
    }

    string PrepareValue(string value)
    {
        if (Options?.UseVarShortcut == true)
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

                if (c == '\'' && b != '\\')
                {
                    inSingleString = !inSingleString;
                }
                else if (c == '"' && b != '\\')
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
                    else if (!IsNameChar(c) && isParsingVarname)
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
        else
        {
            return value;
        }
    }

    string PrepareSelectorUnit(string s)
    {
        StringBuilder output = new StringBuilder();
        char[] chars = s.Trim().ToCharArray();
        bool inSingleString = false;
        bool inDoubleString = false;
        char lastCombinator = '\0';
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            char b = i > 0 ? chars[i - 1] : '\0';

            if (c == '\'' && b != '\\')
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\')
            {
                inDoubleString = !inDoubleString;
            }

            if (inDoubleString || inSingleString)
            {
                output.Append(c);
                continue;
            }

            if (combinators.Contains(c) || char.IsWhiteSpace(c))
            {
                if (char.IsWhiteSpace(lastCombinator) || lastCombinator == '\0')
                    lastCombinator = c;
            }
            else
            {
                bool prettySpace = Options?.Pretty == true && !char.IsWhiteSpace(lastCombinator) && lastCombinator != '\0';
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
}
