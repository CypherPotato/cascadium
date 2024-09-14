using Cascadium.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Cascadium.Entity;

/// <summary>
/// Represents an CSS stylesheet.
/// </summary>
public class CssStylesheet
{
    internal List<string> _statements { get; set; } = new List<string>();
    internal List<CssStylesheet> _stylesheets { get; set; } = new List<CssStylesheet>();
    internal List<CssRule> _rules { get; set; } = new List<CssRule>();

    /// <summary>
    /// Gets the @-rule declaration that holds this stylesheet.
    /// </summary>
    public string? AtRuleDeclaration { get; internal set; }

    /// <summary>
    /// Gets the individual statements of this stylesheet.
    /// </summary>
    public string[] Statements { get => this._statements.ToArray(); }

    /// <summary>
    /// Gets the children <see cref="CssStylesheet"/> of this stylesheet.
    /// </summary>
    public CssStylesheet[] Stylesheets { get => this._stylesheets.ToArray(); }

    /// <summary>
    /// Gets an array of <see cref="CssRule"/> of this stylesheet.
    /// </summary>
    public CssRule[] Rules { get => this._rules.ToArray(); }

    /// <summary>
    /// Gets the used <see cref="CascadiumOptions"/> used to compile this CSS stylesheet.
    /// </summary>
    public CascadiumOptions Options { get; internal set; } = null!;

    /// <summary>
    /// Exports this <see cref="CssStylesheet"/> to an CSS representation string, using the
    /// <see cref="Options"/> parameter.
    /// </summary>
    /// <returns>An CSS string.</returns>
    public string Export()
    {
        return this.Export(this.Options);
    }


    internal CssStylesheet GetOrCreateStylesheet(string atRuleDeclaration, bool canMerge)
    {
        if (canMerge)
        {
            string sanitized = Helper.RemoveSpaces(atRuleDeclaration);
            foreach (CssStylesheet subStylesheet in this._stylesheets)
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
            this._stylesheets.Add(newStylesheet);
            return newStylesheet;
        }
        else
        {
            CssStylesheet newStylesheet = new CssStylesheet()
            {
                AtRuleDeclaration = atRuleDeclaration
            };
            this._stylesheets.Add(newStylesheet);
            return newStylesheet;
        }
    }

    string Export(CascadiumOptions options)
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
            foreach (var rule in css._rules.OrderBy(r => r._order))
            {
                if (options.Pretty) sb.Append(new string(' ', indentLevel * 4));
                sb.Append(rule.Selector);
                if (options.Pretty) sb.Append(' ');
                sb.Append('{');
                if (options.Pretty) sb.Append('\n');

                foreach (KeyValuePair<string, string> property in rule._declarations)
                {
                    if (options.Pretty) sb.Append(new string(' ', (indentLevel + 1) * 4));
                    sb.Append(property.Key);
                    sb.Append(':');

                    if (options.Pretty)
                    {
                        sb.Append(' ');
                        sb.Append(property.Value);
                    }
                    else
                    {
                        string[] propertyValueLnSplitted = property.Value
                            .Split('\n', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

                        sb.Append(string.Join("", propertyValueLnSplitted));
                    }

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

        foreach (string decl in this._statements)
        {
            sb.Append(decl);
            sb.Append(';');
            if (options.Pretty) sb.AppendLine();
        }
        if (options.Pretty && this._statements.Count > 0) sb.AppendLine();

        ExportRules(this, 0);

        foreach (CssStylesheet stylesheet in this._stylesheets)
        {
            ExportStylesheet(stylesheet, 0);
        }

        return sb.ToString().Trim();
    }
}
