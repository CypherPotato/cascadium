using Cascadium.Entity;
using Cascadium.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Compiler;

class Parser
{
    public static NestedStylesheet ParseSpreadsheet(TokenCollection tokens)
    {
        NestedStylesheet spreadsheet = new NestedStylesheet();

    readRule:
        List<string> selectors = new List<string>();

    readRule__nextSelector:
        {
            var next = tokens.Read(out Token result);

            if (!next)
            {
                goto finish;
            }
            else if (result.Type == TokenType.Em_Selector)
            {
                if (result.Content == "")
                    throw new CascadiumException(result.DebugInfo, "empty selectors are not allowed");

                selectors.Add(result.Content);
                goto readRule__nextSelector;
            }
            else if (result.Type == TokenType.Em_RuleStart)
            {
                if (selectors.Count == 0)
                    throw new CascadiumException(result.DebugInfo, "selector expected");

                ReadRule(tokens, spreadsheet, selectors.ToArray());
                selectors.Clear();

                goto readRule__nextSelector;
            }
            else if (result.Type == TokenType.Em_RuleEnd)
            {
                goto readRule;
            }
            else if (result.Type == TokenType.Em_Statement)
            {
                spreadsheet.Statements.Add(result.Content);
                goto readRule__nextSelector;
            }
            else
            {
                throw new CascadiumException(result.DebugInfo, "unexpected token");
            }
        }

    finish:
        return spreadsheet;
    }

    static void ReadRule(TokenCollection tokens, IRuleContainer container, string[] externSelectors)
    {
        int ruleIndex = 0;
        List<string> buildingSelectors = new List<string>();
        NestedRule buildingRule = new NestedRule();

    readRule__nextItem:
        {
            var next = tokens.Read(out Token result);

            if (result.Type == TokenType.Em_RuleEnd)
            {
                goto readRule__finish;
            }
            else if (result.Type == TokenType.Em_RuleStart)
            {
                if (buildingSelectors.Count == 0)
                    throw new CascadiumException(result.DebugInfo, "selector expected");

                ruleIndex++;
                ReadRule(tokens, buildingRule, buildingSelectors.ToArray());

                buildingSelectors.Clear();

                goto readRule__nextItem;
            }
            else if (result.Type == TokenType.Em_PropertyName)
            {
                tokens.Read(out Token valueToken);

                if (valueToken.Type != TokenType.Em_PropertyValue)
                    throw new CascadiumException(tokens.Last.DebugInfo, "property value expected");
                if (string.IsNullOrWhiteSpace(valueToken.Content))
                    goto readRule__nextItem; // skip empty declarations

                buildingRule.Declarations[result.Content] = valueToken.Content;
                goto readRule__nextItem;
            }
            else if (result.Type == TokenType.Em_Selector)
            {
                if (result.Content == "")
                    throw new CascadiumException(result.DebugInfo, "empty selectors are not allowed");
                if (IsSelectorInvalidRuleset(result.Content))
                    throw new CascadiumException(result.DebugInfo, "; expected");

                buildingSelectors.Add(result.Content);
                goto readRule__nextItem;
            }
            else
            {
                throw new CascadiumException(result.DebugInfo, "unexpected token");
            }
        }

    readRule__finish:
        buildingRule.Selectors.AddRange(externSelectors);
        container.Rules.Add(buildingRule);
    }

    static bool IsSelectorInvalidRuleset(string content)
    {
        int nlIndex = content.IndexOfAny(new char[] { '\n', '\r' });
        if (nlIndex >= 0)
        {
            return content.Substring(0, nlIndex).IndexOf(':') >= 0;
        }

        return false;
    }   
}
