using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleCSS;

/// <summary>
/// Provides a CSS compiler that compiles higher-level code with single-line comments, nesting into a legacy CSS file.
/// </summary>
public sealed class SimpleCSSCompiler
{
    private class CssRule
    {
        public string? Selector { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public override String ToString() => $"{Selector} [{Properties.Count}]";
    }

    private string? AtRule { get; set; }
    private List<CssRule> Rules { get; set; } = new List<CssRule>();
    private List<SimpleCSSCompiler> Stylesheets { get; set; } = new List<SimpleCSSCompiler>();
    private List<string> Declarations { get; set; } = new List<string>();

    private CSSCompilerOptions? Options { get; set; }

    private SimpleCSSCompiler() { }

    private string Export()
    {
        StringBuilder sb = new StringBuilder();

        foreach (string decl in Declarations)
        {
            sb.Append(decl);
            if (Options?.Pretty == true) sb.Append("\n");
        }
        if (Options?.Pretty == true) sb.Append("\n");
        ExportRules(sb, this, 0);
        foreach (SimpleCSSCompiler stylesheet in Stylesheets)
        {
            stylesheet.Options = this.Options;
            ExportStylesheet(sb, stylesheet, 0);
        }

        if (Options?.Pretty == true) TrimEndSb(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The byte array including the top module CSS code.</param>
    /// <param name="encoder">The encoder which will be used to decode the CSS output.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified UTF-8 CSS.</returns>
    public static string Compile(ReadOnlySpan<byte> css, Encoding encoder, CSSCompilerOptions? options = null)
    {
        return Compile(encoder.GetString(css), options);
    }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The top module CSS code.</param>
    /// <param name="options">Optional options and parameters to the compilation.</param>
    /// <returns>The compiled and minified CSS.</returns>
    public static string Compile(string css, CSSCompilerOptions? options = null)
    {
        string prepared = PrepareString(css);
        SimpleCSSCompiler s = new SimpleCSSCompiler() { Options = options };
        s.ParseCss(prepared);
        return s.Export().Trim();
    }

    private static void ExportStylesheet(StringBuilder sb, SimpleCSSCompiler css, int indentLevel)
    {
        if (css.AtRule != "")
        {
            if (css.Options?.Pretty == true) sb.Append(new string(' ', indentLevel * 4));
            sb.Append(css.AtRule);
            if (css.Options?.Pretty == true) sb.Append(' ');
            sb.Append('{');
            if (css.Options?.Pretty == true) sb.Append('\n');
        }
        ExportRules(sb, css, indentLevel + 1);
        if (css.Options?.Pretty == true) TrimEndSb(sb);
        if (css.AtRule != "")
        {
            if (css.Options?.Pretty == true) sb.Append('\n');
            if (css.Options?.Pretty == true) sb.Append(new string(' ', indentLevel * 4));
            sb.Append('}');
            if (css.Options?.Pretty == true) sb.Append("\n\n");
        }
    }

    private static void ExportRules(StringBuilder sb, SimpleCSSCompiler css, int indentLevel)
    {
        foreach (var rule in css.Rules)
        {
            if (css.Options?.Pretty == true) sb.Append(new string(' ', indentLevel * 4));
            sb.Append(rule.Selector);
            if (css.Options?.Pretty == true) sb.Append(' ');
            sb.Append('{');
            if (css.Options?.Pretty == true) sb.Append('\n');
            foreach (KeyValuePair<string, string> property in rule.Properties)
            {
                if (css.Options?.Pretty == true) sb.Append(new string(' ', (indentLevel + 1) * 4));
                sb.Append(property.Key);
                sb.Append(':');
                if (css.Options?.Pretty == true) sb.Append(' ');
                sb.Append(property.Value);
                sb.Append(';');
                if (css.Options?.Pretty == true) sb.Append('\n');
            }
            sb.Length--; // remove the last ;
            if (css.Options?.Pretty == true) sb.Append('\n');
            if (css.Options?.Pretty == true) sb.Append(new string(' ', indentLevel * 4));
            sb.Append('}');
            if (css.Options?.Pretty == true) sb.Append("\n\n");
        }
    }

    private static void TrimEndSb(StringBuilder sb)
    {
        int i = sb.Length - 1;

        for (; i >= 0; i--)
            if (!char.IsWhiteSpace(sb[i]))
                break;

        if (i < sb.Length - 1)
            sb.Length = i + 1;
    }

    private void ParseCss(string css)
    {
        bool isAtRule = false;
        int keyLevel = 0;

        StringBuilder sb = new StringBuilder();
        char[] chars = css.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            sb.Append(c);

            if (c == '@' && keyLevel == 0)
            {
                isAtRule = true;
            }
            else if (c == ';' && isAtRule && keyLevel == 0)
            {
                ParseAtRule(sb.ToString());
                isAtRule = false;
                sb.Clear();
            }
            else if (c == '{')
            {
                keyLevel++;
            }
            else
            if (c == '}')
            {
                keyLevel--;
                if (keyLevel == 0)
                {
                    if (isAtRule)
                    {
                        ParseAtRule(sb.ToString());
                        isAtRule = false;
                    }
                    else
                    {
                        ParseRule(sb.ToString(), "");
                    }
                    sb.Clear();
                }
            }
        }
    }

    private string FormatSelector(string current, string before)
    {
        StringBuilder sb = new StringBuilder();
        string[] cSelectors = current.Split(',');
        string[] bSelectors = before.Split(',');

        if (before.Length == 0)
        {
            sb.Append(current);
            goto finish;
        }

        foreach (string C in cSelectors)
        {
            string c = C.Trim();
            foreach (string B in bSelectors)
            {
                string b = B.Trim();
                if (c.StartsWith('&'))
                {
                    sb.Append(b);
                    sb.Append(c.Substring(1).TrimStart());
                }
                else
                {
                    sb.Append(b);
                    sb.Append(' ');
                    sb.Append(c);
                }
                sb.Append(',');
            }
        }

        sb.Length--;

    finish:
        string[] parts = sb.ToString()
            .Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Join(' ', parts);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseAtRule(string atRuleStr)
    {
        string ruleStr = atRuleStr.Trim();
        int openingTagIndex = ruleStr.IndexOf('{');

        if (ruleStr.Length == 0) return; // empty @ rule

        if (openingTagIndex >= 0)
        {
            SimpleCSSCompiler css = new SimpleCSSCompiler();
            css.AtRule = ruleStr.Substring(0, openingTagIndex);
            string body = ruleStr
                .Substring(openingTagIndex + 1, ruleStr.Length - openingTagIndex - 2)
                .Trim();

            css.ParseCss(body);
            this.Stylesheets.Add(css);
        }
        else
        {
            this.Declarations.Add(ruleStr);
        }
    }

    private void ParseRule(string ruleStr, string baseSelector)
    {
        ruleStr = ruleStr.Trim();
        CssRule rule = new CssRule();
        string mounting = "";

        void SetDeclaration()
        {
            string declaration = mounting.Substring(0, mounting.Length - 1).Trim();
            int sepIndex = declaration.IndexOf(':');
            if (sepIndex > 0)
            {
                string propKey = declaration.Substring(0, sepIndex).Trim();
                string propValue = declaration.Substring(sepIndex + 1).Trim();
                propValue = PrepareValue(propValue);
                rule.Properties[propKey] = propValue;
                mounting = "";
            }
        }

        int openingTagIndex = ruleStr.IndexOf('{');

        rule.Selector = ruleStr.Substring(0, openingTagIndex).Trim();
        rule.Selector = FormatSelector(rule.Selector, baseSelector);

        int keyState = 0;

        bool inSingleString = false;
        bool inDoubleString = false;

        string body = ruleStr.Substring(openingTagIndex + 1);
        for (int i = 0; i < body.Length; i++)
        {
            char c = body[i];
            char b = i > 0 ? body[i - 1] : '\0';
            mounting += c;

            if (c == '\'' && b != '\\')
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\')
            {
                inDoubleString = !inDoubleString;
            }

            if (inSingleString || inDoubleString)
                continue;

            if (c == '{')
            {
                keyState++;
            }
            else if (c == '}')
            {
                keyState--;
                if (keyState == 0)
                {
                    ParseRule(mounting, rule.Selector);
                    mounting = "";
                    continue;
                }
            }
            else if (c == ';' && keyState == 0)
            {
                SetDeclaration();
            }
        }

        if (mounting.Length > 0 && mounting.Contains(':') && mounting.EndsWith('}'))
        {
            SetDeclaration();
        }

        if (rule.Properties.Count > 0)
            Rules.Add(rule);
    }

    private string PrepareValue(string value)
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

    private static bool IsNameChar(char c)
    {
        return Char.IsLetter(c) || Char.IsDigit(c) || c == '_' || c == '-';
    }

    private static string PrepareString(string input)
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
}