using Cascadium.Object;
using System;
using System.Text;

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
            for (int i = 0; i < this.Position; i++)
            {
                if (this.InputString[i] == '\n')
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
            for (int n = 0; n < this.Position; n++)
            {
                if (this.InputString[n] == '\n')
                {
                    col = 0;
                }
                col++;
            }
            return col;
        }
    }

    public TextInterpreter(string s)
    {
        this.InputString = s;
        this.Length = this.InputString.Length;
    }

    public TokenDebugInfo TakeSnapshot(string text)
    {
        int textIndex = this.InputString.Substring(0, this.Position).LastIndexOf(text.Trim());
        return this.TakeSnapshot(-(this.Position - textIndex));
    }

    public TokenDebugInfo TakeSnapshot(int offset = 0)
    {
        this.Move(offset);
        var snapshot = new TokenDebugInfo()
        {
            Column = this.Column,
            Line = this.Line
        };
        this.Move(offset * -1);

        return snapshot;
    }

    public bool CanRead()
    {
        return this.Position < this.InputString.Length - 1;
    }

    public void Move(int count)
    {
        this.Position = Math.Min(Math.Max(this.Position + count, 0), this.InputString.Length);
    }

    public int Read(out char c)
    {
        if (this.InputString.Length <= this.Position)
        {
            c = '\0';
            return -1;
        }
        c = this.InputString[this.Position];
        this.Position++;
        return 1;
    }

    public string ReadAtLeast(int count)
    {
        StringBuilder sb = new StringBuilder();

        int n = 0;
        while (n < count)
        {
            int j = this.Read(out char c);
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

        while (this.Read(out char c) > 0)
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
            if (this.Read(out char c) > 0)
            {
                if (Token.IsWhitespaceChr(c))
                {
                    continue; // whitespace
                }
                else
                {
                    this.Move(-1);
                    break;
                }
            }
            else break;
        }
    }
}
