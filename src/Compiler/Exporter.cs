using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cascadium.Compiler;
internal class Exporter : CompilerModule
{
    public Exporter(CompilerContext context) : base(context)
    {
    }

    public string Export()
    {
        StringBuilder sb = new StringBuilder();

        if (this.Options?.Merge == true)
        {
            this.Context.Merger.Merge(true, true);
        }

        foreach (string decl in Context.Declarations)
        {
            sb.Append(decl);
            if (Options?.Pretty == true) sb.Append("\n");
        }
        if (Options?.Pretty == true) sb.Append("\n");

        ExportRules(sb, this.Context, 0);

        foreach (CompilerContext stylesheet in Context.Stylesheets)
        {
            stylesheet.Options = this.Options;
            ExportStylesheet(sb, stylesheet, 0);
        }

        if (Options?.Pretty == true) this.Context.Utils.TrimEndSb(sb);
        return sb.ToString();
    }

    public void ExportStylesheet(StringBuilder sb, CompilerContext css, int indentLevel)
    {
        if (css.AtRule != "")
        {
            if (css.Options?.Pretty == true) sb.Append(new string(' ', indentLevel * 4));
            sb.Append(css.AtRule?.Trim());
            if (css.Options?.Pretty == true) sb.Append(' ');
            sb.Append('{');
            if (css.Options?.Pretty == true) sb.Append('\n');
        }
        foreach (string decl in css.Declarations)
        {
            if (css.Options?.Pretty == true) sb.Append(new string(' ', indentLevel + 1 * 4));
            sb.Append(decl);
            if (css.Options?.Pretty == true) sb.Append("\n");
        }
        ExportRules(sb, css, indentLevel + 1);
        if (css.Options?.Pretty == true) this.Context.Utils.TrimEndSb(sb);
        if (css.AtRule != "")
        {
            if (css.Options?.Pretty == true) sb.Append('\n');
            if (css.Options?.Pretty == true) sb.Append(new string(' ', indentLevel * 4));
            sb.Append('}');
            if (css.Options?.Pretty == true) sb.Append("\n\n");
        }
    }

    private void ExportRules(StringBuilder sb, CompilerContext css, int indentLevel)
    {
        foreach (var rule in css.Rules.OrderBy(r => r.Order))
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
}
