using Cascadium.Object;
using System.Collections.Generic;

namespace Cascadium.Compiler;

internal class Tokenizer
{
    public TextInterpreter Interpreter { get; }

    public Tokenizer(string code)
    {
        this.Interpreter = new TextInterpreter(code);
    }

    public void Tokenize(TokenBuffer collection)
    {
        char[] hitChars = new char[] { Token.Ch_BraceOpen, Token.Ch_BraceClose, Token.Ch_Semicolon };
        List<Token> tokens = new List<Token>();
        int opennedRules = 0;
        TokenDebugInfo lastOpennedRule = default;

        string result;
        char hit;
        while ((hit = this.Interpreter.ReadUntil(hitChars, true, out result)) != '\0')
        {
            if (hit == Token.Ch_Semicolon)
            {
                tokens.AddRange(this.ReadCurrentDeclaration(result));
                continue;
            }
            else if (hit == Token.Ch_BraceOpen)
            {
                tokens.AddRange(this.ReadSelectors(result));
                tokens.Add(new Token(TokenType.Em_RuleStart, "", this.Interpreter));

                opennedRules++;
                lastOpennedRule = this.Interpreter.TakeSnapshot(-1);

                continue;
            }
            else if (hit == Token.Ch_BraceClose)
            {
                if (!string.IsNullOrWhiteSpace(result))
                {
                    // remaining declaration
                    tokens.AddRange(this.ReadCurrentDeclaration(result));
                }

                tokens.Add(new Token(TokenType.Em_RuleEnd, "", this.Interpreter));
                opennedRules--;
                continue;
            }
        }

        if (hit == '\0')
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                throw new CascadiumException(this.Interpreter.TakeSnapshot(result), this.Interpreter.InputString, "syntax error: unexpected token");
            }
        }

        if (opennedRules != 0)
        {
            throw new CascadiumException(lastOpennedRule, this.Interpreter.InputString, "syntax error: unclosed rule");
        }

        collection.Write(tokens);
    }

    IEnumerable<Token> ReadCurrentDeclaration(string declaration)
    {
        if (declaration.TrimStart().StartsWith('@'))
        {
            // its an statement
            yield return new Token(TokenType.Em_Statement, declaration.Trim(), this.Interpreter);
            yield break;
        }

        int dotPos = declaration.IndexOf(Token.Ch_DoubleDots);
        if (dotPos == -1)
        {
            throw new CascadiumException(this.Interpreter.TakeSnapshot(declaration), this.Interpreter.InputString, "syntax error: unexpected token \"" + declaration.Trim() + "\"");
        }

        string property = declaration.Substring(0, dotPos).Trim();
        string value = declaration.Substring(dotPos + 1).Trim();

        if (!Token.IsValidPropertyName(property))
        {
            throw new CascadiumException(this.Interpreter.TakeSnapshot(declaration), this.Interpreter.InputString, "syntax error: invalid property name");
        }
        else if (Token.IsPropertyValueUnescapedDoubleDots(value))
        {
            throw new CascadiumException(this.Interpreter.TakeSnapshot(declaration), this.Interpreter.InputString, "syntax error: unclosed declaration");
        }
        else
        {
            yield return new Token(TokenType.Em_PropertyName, property, this.Interpreter);
            yield return new Token(TokenType.Em_PropertyValue, value, this.Interpreter);
        }
    }

    IEnumerable<Token> ReadSelectors(string selectorCode)
    {
        if (selectorCode.IndexOf(',') < 0)
        {
            yield return new Token(TokenType.Em_Selector, selectorCode.Trim(), this.Interpreter);
        }
        else
        {
            string[] selectors = Helper.SafeSplit(selectorCode, ',');
            foreach (string s in selectors)
            {
                yield return new Token(TokenType.Em_Selector, s.Trim(), this.Interpreter);
            }
        }
    }
}
