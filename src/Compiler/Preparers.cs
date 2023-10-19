using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;

internal class Preparers : CompilerModule
{
    public Preparers(CompilerContext context) : base(context)
    {
    }

    public string PrepareCssInput(string input)
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

    public string PrepareValue(string value)
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
                    else if (!Context.Utils.IsNameChar(c) && isParsingVarname)
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

    public bool IsSelectorsEqual(string? a, string? b)
    {
        if (a == null || b == null) return a == b;
        return PrepareSelectorUnit(a) == PrepareSelectorUnit(b);
    }

    public string PrepareSelectorUnit(string s)
    {
        StringBuilder output = new StringBuilder();
        char[] chars;

        bool ignoreTrailingSpace = Options == null || Options.KeepNestingSpace == false;

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

            if (CascadiumCompiler.combinators.Contains(c) || char.IsWhiteSpace(c))
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
