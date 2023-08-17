
using System.Runtime.CompilerServices;
using System.Text;

namespace CypherPotato;

/// <summary>
/// Provides a CSS compiler that compiles higher-level code with single-line comments, nesting into a legacy CSS file.
/// </summary>
public sealed class SimpleCSSCompiler
{
    private class CssRule
    {
        public string? Selector { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    private string? AtRule { get; set; }
    private List<CssRule> Rules { get; set; } = new List<CssRule>();
    private List<SimpleCSSCompiler> Stylesheets { get; set; } = new List<SimpleCSSCompiler>();
    private List<string> Declarations { get; set; } = new List<string>();

    private SimpleCSSCompiler() { }

    private string Export()
    {
        StringBuilder sb = new StringBuilder();

        foreach (string decl in Declarations)
        {
            sb.Append(decl);
        }
        ExportRules(sb, this.Rules);
        foreach (SimpleCSSCompiler stylesheet in Stylesheets)
        {
            ExportStylesheet(sb, stylesheet);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The byte array including the top module CSS code.</param>
    /// <param name="encoder">The encoder which will be used to decode the CSS output.</param>
    /// <returns>The compiled and minified UTF-8 CSS.</returns>
    public static string Compile(ReadOnlySpan<byte> css, Encoding encoder)
    {
        return Compile(encoder.GetString(css));
    }

    /// <summary>
    /// Compiles a top-module CSS stylesheet to legacy CSS, with the code already minified.
    /// </summary>
    /// <param name="css">The top module CSS code.</param>
    /// <returns>The compiled and minified CSS.</returns>
    public static string Compile(string css)
    {
        string prepared = PrepareString(css);
        SimpleCSSCompiler s = new SimpleCSSCompiler();
        s.ParseCss(prepared);
        return s.Export();
    }

    private static void ExportStylesheet(StringBuilder sb, SimpleCSSCompiler css)
    {
        if (css.AtRule != "")
        {
            sb.Append(css.AtRule);
            sb.Append('{');
        }
        ExportRules(sb, css.Rules);
        if (css.AtRule != "") sb.Append('}');
    }

    private static void ExportRules(StringBuilder sb, IEnumerable<CssRule> rules)
    {
        foreach (var rule in rules)
        {
            sb.Append(rule.Selector);
            sb.Append('{');
            foreach (KeyValuePair<string, string> property in rule.Properties)
            {
                sb.Append(property.Key);
                sb.Append(':');
                sb.Append(property.Value);
                sb.Append(';');
            }
            sb.Length--; // remove the last ;
            sb.Append('}');
        }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsProperty(string mount)
    {
        string trimmed = mount.Trim();
        if (trimmed.Length == 0) return false;
        if (Char.IsLetter(trimmed[0]) || trimmed[0] == '-') return true;
        return false;
    }

    private string FormatSelector(string current, string before)
    {
        StringBuilder sb = new StringBuilder();
        string[] cSelectors = current.Split(',');
        string[] bSelectors = before.Split(',');

        if (before.Length == 0)
            return current;

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
        return sb.ToString();
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

        int openingTagIndex = ruleStr.IndexOf('{');

        rule.Selector = ruleStr.Substring(0, openingTagIndex).Trim();
        rule.Selector = FormatSelector(rule.Selector, baseSelector);

        int keyState = 0;
        string mounting = "";
        string propKey = "";

        string body = ruleStr.Substring(openingTagIndex + 1);
        for (int i = 0; i < body.Length; i++)
        {
            char c = body[i];
            mounting += c;

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
                    propKey = "";
                    mounting = "";
                    continue;
                }
            }

            if (keyState > 0) continue;

            if (c == ':' && IsProperty(mounting) && propKey == "")
            {
                propKey = mounting;
                mounting = "";
            }
            else if (c == ';')
            {
                string name = propKey.Trim().TrimEnd(':'), value = mounting.Trim().TrimEnd(';');
                rule.Properties.Add(name, value);
                propKey = "";
                mounting = "";
            }
        }

        if (rule.Properties.Count > 0)
            Rules.Insert(0, rule);
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
            char before = inputChars[Math.Max(0, i - 1)];

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
                output.Length--;
            }
            else if (current == '/' && before == '*' && !inString() && !inSinglelineComment)
            {
                inMultilineComment = false;
                output.Length--;
                continue;
            }
            else if (current == '/' && before == '/' && !inString() && !inMultilineComment)
            {
                inSinglelineComment = true;
                output.Length--;
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