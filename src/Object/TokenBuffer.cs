using System;
using System.Collections.Generic;

namespace Cascadium.Object;

internal class TokenBuffer
{
    private readonly List<Token> tokens = new List<Token>();

    public int Position { get; set; }
    public Token Last { get => tokens[tokens.Count - 1]; }
    public Token Current { get => tokens[Math.Min(Position, tokens.Count - 1)]; }

    public void Write(in Token token)
    {
        tokens.Add(token);
    }

    public void Write(IEnumerable<Token> tokens)
    {
        this.tokens.AddRange(tokens);
    }

    public bool Read(out Token token)
    {
        bool state;
        if (Position < tokens.Count)
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
