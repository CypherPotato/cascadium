using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleCSS;
public sealed partial class SimpleCSSCompiler
{
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

            if (c == '@' && keyLevel == 0 && sb.Length == 1)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseAtRule(string atRuleStr)
    {
        string ruleStr = atRuleStr.Trim();
        int openingTagIndex = ruleStr.IndexOf('{');

        if (ruleStr.Length == 0) return; // empty @ rule

        if (openingTagIndex >= 0)
        {
            SimpleCSSCompiler css = new SimpleCSSCompiler()
            {
                Options = this.Options
            };
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
                bool wasConverted = false;
                string propKey = declaration.Substring(0, sepIndex).Trim();
                string? propValue = declaration.Substring(sepIndex + 1).Trim();
                propValue = PrepareValue(propValue);

                if (Options?.Converters != null)
                {
                    foreach (CSSConverter converter in Options.Converters)
                    {
                        if (converter.CanConvert(propKey, propValue))
                        {
                            wasConverted = true;
                            NameValueCollection result = new NameValueCollection();
                            converter.Convert(propValue, result);
                            foreach (string rKey in result)
                            {
                                string rValue = result[rKey]!;
                                rule.Properties[rKey] = rValue;
                            }
                        }
                    }
                }
                if (!wasConverted)
                {
                    if (propValue == "")
                    {
                        mounting = "";
                        return; // do not add empty declarations
                    }
                    else
                    {
                        rule.Properties[propKey] = propValue;
                    }
                }

                mounting = "";
            }
        }

        int openingTagIndex = ruleStr.IndexOf('{');

        rule.Selector = ruleStr.Substring(0, openingTagIndex).Trim();
        rule.Selector = JoinSelector(rule.Selector, baseSelector);

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

    private string JoinSelector(string current, string before)
    {
        StringBuilder sb = new StringBuilder();
        string[] cSelectors = current.Split(',');
        string[] bSelectors = before.Split(',');

        if (before.Length == 0)
        {
            foreach (string cSelector in cSelectors)
            {
                string prepared = PrepareSelectorUnit(cSelector);
                sb.Append(prepared);
                sb.Append(',');
            }
            goto finish;
        }

        foreach (string C in cSelectors)
        {
            string c = C.Trim();
            foreach (string B in bSelectors)
            {
                string b = B.Trim();
                string s;
                if (c.StartsWith('&'))
                {
                    sb.Append(b);
                    s = c.Substring(1).TrimStart();
                }
                else
                {
                    sb.Append(b);
                    if (!combinators.Contains(c[0]))
                    {
                        sb.Append(' ');
                    }
                    s = c;
                }
                s = PrepareSelectorUnit(s);
                sb.Append(s);
                sb.Append(',');
            }
        }

    finish:
        sb.Length--;
        return sb.ToString();
    }
}
