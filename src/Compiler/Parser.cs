﻿using Cascadium.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;
internal class Parser : CompilerModule
{
    public Parser(CompilerContext context) : base(context)
    {
    }

    public bool ParseXCss(string css)
    {
        bool anyParsed = false;
        bool selectorStarted = false;
        bool isAtRule = false;
        int keyLevel = 0;

        StringBuilder sb = new StringBuilder();
        char[] chars = css.ToCharArray();

        bool inSingleString = false;
        bool inDoubleString = false;

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i], b = i > 0 ? chars[i - 1] : '\0'; ;
            sb.Append(c);

            if (!char.IsWhiteSpace(c))
            {
                selectorStarted = true;
            }

            if (c == '\'' && b != '\\' && !inDoubleString)
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\' && !inSingleString)
            {
                inDoubleString = !inDoubleString;
            }

            if (inSingleString || inDoubleString)
                continue;

            if (c == '@' && keyLevel == 0 && selectorStarted)
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
            else if (c == '}')
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
                    anyParsed = true;
                    sb.Clear();
                }
                selectorStarted = false;
            }
        }
        return anyParsed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseAtRule(in string atRuleStr, string? baseSelector = null)
    {
        string ruleStr = atRuleStr.Trim();
        int openingTagIndex = ruleStr.IndexOf('{');

        if (ruleStr.Length == 0) return; // empty @ rule

        if (baseSelector != null)
        {
            ruleStr = ruleStr.Substring(0, openingTagIndex + 1)
                + baseSelector + '{' + ruleStr.Substring(openingTagIndex + 1);

            ruleStr += '}';
        }

        if (openingTagIndex >= 0)
        {
            CompilerContext css = new CompilerContext()
            {
                Options = this.Options
            };

            css.AtRule = ruleStr.Substring(0, openingTagIndex);

            if (Options != null)
            {
                string sanitized = css.AtRule.Trim().TrimStart('@');
                foreach (string atRule in Options.AtRulesRewrites)
                {
                    if (string.Compare(atRule, sanitized, true) == 0)
                    {
                        css.AtRule = "@" + Options.AtRulesRewrites[atRule];
                    }
                }
            }

            string body = ruleStr
                .Substring(openingTagIndex + 1, ruleStr.Length - openingTagIndex - 2)
                .Trim();

            bool parseResult = css.Parser.ParseXCss(body);
            if (parseResult)
            {
                // body was interpreted as an css stylesheet
                this.Context.Childrens.Add(css);
            }
            else
            {
                // is an at-rule, but not identified as an stylesheet inside it
                // try to interpret as a rule
                ParseRule(ruleStr, "");
            }
        }
        else
        {
            this.Context.Declarations.Add(ruleStr);
        }
    }

    private void ParseRule(string ruleStr, string baseSelector)
    {
        ruleStr = ruleStr.Trim();
        Rule rule = new Rule()
        {
            Order = Context.StackOrder++
        };

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
                propValue = Context.Preparers.PrepareValue(propValue);

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

            if (c == '\'' && b != '\\' && !inDoubleString)
            {
                inSingleString = !inSingleString;
            }
            else if (c == '"' && b != '\\' && !inSingleString)
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
                    if (mounting.TrimStart().StartsWith('@'))
                    {
                        // at rule inside rule
                        ParseAtRule(mounting, rule.Selector);
                    }
                    else
                    {
                        ParseRule(mounting, rule.Selector);
                    }
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
            Context.Rules.Add(rule);
    }

    private string JoinSelector(string current, string before)
    {
        StringBuilder sb = new StringBuilder();
        string[] cSelectors = Context.Split.SafeSplit(current, ',');
        string[] bSelectors = Context.Split.SafeSplit(before, ',');

        if (before.Length == 0)
        {
            foreach (string cSelector in cSelectors)
            {
                string prepared = Context.Preparers.PrepareSelectorUnit(cSelector);
                sb.Append(prepared);
                sb.Append(',');
                if (Options?.Pretty == true)
                    sb.Append(' ');
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
                    s = c.Substring(1);
                    if (Options == null || Options.KeepNestingSpace == false)
                    {
                        s = s.TrimStart();
                    }
                }
                else
                {
                    sb.Append(b);
                    if (c.Length != 0 && !CascadiumCompiler.combinators.Contains(c[0]))
                    {
                        sb.Append(' ');
                    }
                    s = c;
                }
                s = Context.Preparers.PrepareSelectorUnit(s);
                sb.Append(s);
                sb.Append(',');
                if (Options?.Pretty == true)
                    sb.Append(' ');
            }
        }

    finish:
        if (sb.Length > 0) sb.Length--;
        if (sb.Length > 0 && Options?.Pretty == true) sb.Length--;
        return sb.ToString();
    }
}
