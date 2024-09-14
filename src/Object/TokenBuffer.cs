using System;
using System.Collections.Generic;

namespace Cascadium.Object;

internal class TokenBuffer
{
    private readonly List<Token> tokens = new List<Token>();

    public int Position { get; set; }
    public Token Last { get => this.tokens[this.tokens.Count - 1]; }
    public Token Current { get => this.tokens[Math.Min(this.Position, this.tokens.Count - 1)]; }

    public void Write(IEnumerable<Token> tokens)
    {
        this.tokens.AddRange(tokens);
    }

    public bool Read(out Token token)
    {
        bool state;
        if (this.Position < this.tokens.Count)
        {
            token = this.tokens[this.Position];
            state = true;
        }
        else
        {
            token = default;
            state = false;
        }
        this.Position++;
        return state;
    }
}
