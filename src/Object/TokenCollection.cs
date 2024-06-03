using System;

namespace Cascadium.Object;

internal class TokenCollection
{
    private Token[] tokens;

    public int Position { get; set; }
    public Token Last { get => tokens[tokens.Length - 1]; }
    public Token Current { get => tokens[Math.Min(Position, tokens.Length - 1)]; }

    public TokenCollection(Token[] tokens)
    {
        this.tokens = tokens;
    }

    public bool Expect(TokenType type, out Token result)
    {
        bool state = Read(out result);
        if (!state)
        {
            return false;
        }
        if (result.Type != type)
        {
            throw new CascadiumException(result.DebugInfo, $"expected {type}. got {result.Type} instead.");
        }
        return true;
    }

    public bool Read(out Token token)
    {
        bool state;
        if (Position < tokens.Length)
        {
            token = tokens[Position];
            state = true;
        }
        else
        {
            token = default;
            state = false;
        }
        Position++;
        return state;
    }
}
