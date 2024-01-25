using Cascadium.Compiler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Entity;

class CssStylesheet
{
    public string? AtRuleDeclaration { get; set; }
    public List<string> Statements { get; set; } = new List<string>();
    public List<CssStylesheet> Stylesheets { get; set; } = new List<CssStylesheet>();
    public List<CssRule> Rules { get; set; } = new List<CssRule>();

    public CssStylesheet GetOrCreateStylesheet(string atRuleDeclaration, bool canMerge)
    {
        if (canMerge)
        {
            string sanitized = Helper.RemoveSpaces(atRuleDeclaration);
            foreach (CssStylesheet subStylesheet in Stylesheets)
            {
                if (Helper.RemoveSpaces(subStylesheet.AtRuleDeclaration ?? "") == sanitized)
                {
                    return subStylesheet;
                }
            }

            CssStylesheet newStylesheet = new CssStylesheet()
            {
                AtRuleDeclaration = atRuleDeclaration
            };
            Stylesheets.Add(newStylesheet);
            return newStylesheet;
        }
        else
        {
            CssStylesheet newStylesheet = new CssStylesheet()
            {
                AtRuleDeclaration = atRuleDeclaration
            };
            Stylesheets.Add(newStylesheet);
            return newStylesheet;
        }
    }

    public string Export(CascadiumOptions options)
    {
        StringBuilder sb = new StringBuilder();

        void ExportStylesheet(CssStylesheet css, int indentLevel)
        {
            if (css.AtRuleDeclaration != null)
            {
                if (options.Pretty) sb.Append(new string(' ', indentLevel * 4));
                sb.Append(css.AtRuleDeclaration.Trim());
                if (options.Pretty) sb.Append(' ');
                sb.Append('{');
                if (options.Pretty) sb.Append('\n');
            }
            ExportRules(css, indentLevel + 1);
            if (options.Pretty)
            {
                sb = new StringBuilder(sb.ToString().TrimEnd());
            }
            if (css.AtRuleDeclaration != null)
            {
                if (options.Pretty) sb.Append('\n');
                if (options.Pretty) sb.Append(new string(' ', indentLevel * 4));
                sb.Append('}');
                if (options.Pretty) sb.Append("\n\n");
            }
        }

        void ExportRules(CssStylesheet css, int indentLevel)
        {
            foreach (var rule in css.Rules.OrderBy(r => r.Order))
            {
                if (options.Pretty) sb.Append(new string(' ', indentLevel * 4));
                sb.Append(rule.Selector);
                if (options.Pretty) sb.Append(' ');
                sb.Append('{');
                if (options.Pretty) sb.Append('\n');

                foreach (KeyValuePair<string, string> property in rule.Declarations)
                {
                    if (options.Pretty) sb.Append(new string(' ', (indentLevel + 1) * 4));
                    sb.Append(property.Key);
                    sb.Append(':');
                    if (options.Pretty) sb.Append(' ');
                    sb.Append(property.Value);
                    sb.Append(';');
                    if (options.Pretty) sb.Append('\n');
                }

                sb.Length--; // remove the last ;
                if (options.Pretty) sb.Append('\n');
                if (options.Pretty) sb.Append(new string(' ', indentLevel * 4));
                sb.Append('}');
                if (options.Pretty) sb.Append("\n\n");
            }
        }

        foreach (string decl in Statements)
        {
            sb.Append(decl);
            sb.Append(';');
            if (options.Pretty) sb.AppendLine();
        }
        if (options.Pretty && Statements.Count > 0) sb.AppendLine();

        ExportRules(this, 0);

        foreach (CssStylesheet stylesheet in Stylesheets)
        {
            ExportStylesheet(stylesheet, 0);
        }

        return sb.ToString().Trim();
    }
}
