using System.Text;
using Cascadium.Object;

namespace Cascadium.Compiler;

class TextInterpreter
{
    public string InputString { get; private set; }
    public int Position { get; private set; } = 0;
    public int Length { get; private set; }
    public int Line
    {
        get
        {
            int ocurrences = 1; // line start at 1
            for (int i = 0; i < Position; i++)
            {
                if (InputString[i] == '\n')
                {
                    ocurrences++;
                }
            }
            return ocurrences;
        }
    }

    public int Column
    {
        get
        {
            int col = 1;
            for (int n = 0; n < Position; n++)
            {
                if (InputString[n] == '\n')
                {
                    col = 0;
                }
                col++;
            }
            return col;
        }
    }

    public string CurrentLine
    {
        get
        {
            return InputString.Split('\n')[Line - 1];
        }
    }

    public TextInterpreter(string s)
    {
        InputString = s;
        Length = InputString.Length;
    }

    public TokenDebugInfo TakeSnapshot(string text)
    {
        int textIndex = InputString.Substring(0, Position).LastIndexOf(text.Trim());
        return TakeSnapshot(-(Position - textIndex));
    }

    public TokenDebugInfo TakeSnapshot(int offset = 0)
    {
        Move(offset);
        var snapshot = new TokenDebugInfo()
        {
            Column = Column,
            Line = Line,
            LineText = CurrentLine
        };
        Move(offset * -1);

        return snapshot;
    }

    public bool CanRead()
    {
        return Position < InputString.Length - 1;
    }

    public void Move(int count)
    {
        Position = Math.Min(Math.Max(Position + count, 0), InputString.Length);
    }

    public int Read(out char c)
    {
        if (InputString.Length <= Position)
        {
            c = '\0';
            return -1;
        }
        c = InputString[Position];
        Position++;
        return 1;
    }

    public string ReadAtLeast(int count)
    {
        StringBuilder sb = new StringBuilder();

        int n = 0;
        while (n < count)
        {
            int j = Read(out char c);
            if (j >= 0)
            {
                sb.Append(c);
                n++;
            }
            else break;
        }

        return sb.ToString();
    }

    public char ReadUntil(Span<char> untilChars, bool wrapStringToken, out string result)
    {
        char hit = '\0';
        StringBuilder sb = new StringBuilder();

        bool inDoubleString = false;
        bool inSingleString = false;
        char b = '\0';

        while (Read(out char c) > 0)
        {
            if (wrapStringToken && !inSingleString && c == Token.Ch_DoubleStringQuote && b != Token.Ch_CharEscape)
            {
                inDoubleString = !inDoubleString;
            }
            else if (wrapStringToken && !inDoubleString && c == Token.Ch_SingleStringQuote && b != Token.Ch_CharEscape)
            {
                inSingleString = !inSingleString;
            }

            if (inDoubleString || inSingleString)
            {
                sb.Append(c);
                b = c;
                continue;
            }

            if (untilChars.Contains(c))
            {
                hit = c;
                break;
            }

            sb.Append(c);
            b = c;
        }

        result = sb.ToString();
        return hit;
    }

    public void SkipIgnoreTokens()
    {
        bool skipping = true;
        while (skipping)
        {
            if (Read(out char c) > 0)
            {
                if (Token.IsWhitespaceChr(c))
                {
                    continue; // whitespace
                }
                else
                {
                    Move(-1);
                    break;
                }
            }
            else break;
        }
    }
}
