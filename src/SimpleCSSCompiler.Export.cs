using System.Text;

namespace SimpleCSS;
public sealed partial class SimpleCSSCompiler
{
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

    private static void ExportStylesheet(StringBuilder sb, SimpleCSSCompiler css, int indentLevel)
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
}
