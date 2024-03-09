using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cascadium.Compiler;

namespace Cascadium.Object;

struct TokenDebugInfo
{
    public int Line;
    public int Column;
    public string LineText;
}

struct Token
{
    public static readonly char[] Sel_Combinators = new[] { '>', '~', '+' };

    public static readonly char Ch_DoubleStringQuote = '"';
    public static readonly char Ch_SingleStringQuote = '\'';
    public static readonly char Ch_CharEscape = '\\';

    public static readonly char Ch_BraceOpen = '{';
    public static readonly char Ch_BraceClose = '}';

    public static readonly char Ch_ParentesisOpen = '(';
    public static readonly char Ch_ParentesisClose = ')';

    public static readonly char Ch_Comma = ',';
    public static readonly char Ch_Semicolon = ';';
    public static readonly char Ch_DoubleDots = ':';

    public static bool IsPropertyValueUnescapedDoubleDots(string propertyValue)
    {
        return Helper.SafeCountIncidences(propertyValue, ':') > 0;
    }

    public static bool IsValidPropertyName(string propertyName)
    {
        if (propertyName.Length == 0) return false;
        return propertyName.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '$' || c == '%'); // $ and % is an special token for converters
    }

    public static bool IsIdentifierChr(char c)
    {
        return Char.IsLetter(c) || Char.IsDigit(c) || c == '_' || c == '-';
    }

    public static bool IsWhitespaceChr(char c)
    {
        return c == ' ' || c == '\t' || c == '\r' || c == '\t' || c == '\n';
    }

    public string Content { get; }
    public TokenType Type { get; } = TokenType.Empty;
    public TokenDebugInfo DebugInfo { get; }

    public Token(TokenType type, string content, TextInterpreter raiser)
    {
        Type = type;
        Content = content;
        DebugInfo = raiser.TakeSnapshot(-content.Length);
    }

    public override string ToString() => $"{{{Type}}} \"{Content}\"";
}

enum TokenType
{
    Empty = -1,

    Em_Comma = 21,
    Em_Semicolon = 22,
    Em_DoubleDots = 23,

    Em_Selector = 27,
    Em_PropertyName = 30,
    Em_PropertyValue = 31,
    Em_Statement = 35,

    Em_RuleEnd = 0,
    Em_RuleStart = 1
}
