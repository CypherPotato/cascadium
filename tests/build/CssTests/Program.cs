using ExCSS;
using Cascadium;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CssTests;

internal class Program
{
    static void Main(string[] args)
    {
        var tests = GetAttributedTypes<CssTestAttribute>(Assembly.GetExecutingAssembly());

        Directory.CreateDirectory("output");

        foreach (var testMember in tests)
        {
            var test = (SimpleCssTest)Activator.CreateInstance(testMember)!;
            string result = test.Run();
            File.WriteAllText("output/" + testMember.Name + ".md", result);
            Console.WriteLine("Test run: " + testMember.Name);
        }
    }

    static IEnumerable<Type> GetAttributedTypes<T>(Assembly assembly) where T : Attribute
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(T), true).Length > 0)
            {
                yield return type;
            }
        }
    }
}

public class CssTestAttribute : Attribute
{
}

public abstract class SimpleCssTest
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Input { get; }
    public abstract CascadiumOptions Options { get; set; }

    public string Run()
    {
        Options.Pretty = true;
        string cssPretty = Cascadium.CascadiumCompiler.Compile(Input, Options);
        string settings = JsonSerializer.Serialize(Options, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        Options.Pretty = false;
        string cssUgly = Cascadium.CascadiumCompiler.Compile(Input, Options);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# " + Name + "\n\n");
        sb.AppendLine(Description);
        sb.AppendLine();
        sb.AppendLine($"Input ({Input.Length} chars):");
        sb.AppendLine();
        sb.AppendLine("```scss\n" + Input + "\n```\n\n");
        sb.AppendLine($"Output (indented, {cssPretty.Length} chars):");
        sb.AppendLine();
        sb.AppendLine("```css\n" + cssPretty + "\n```\n\n");
        sb.AppendLine($"Output (min, {cssUgly.Length} chars):");
        sb.AppendLine();
        sb.AppendLine("```css\n" + cssUgly + "\n```");
        sb.AppendLine();
        sb.AppendLine("\n--------\nExcss parsing results:\n\n");
        sb.AppendLine($"- Indented: {IsValidCss(cssPretty)}");
        sb.AppendLine($"- Ugly: {IsValidCss(cssUgly)}");
        sb.AppendLine();
        sb.AppendLine("\n--------\nConfiguration used:\n\n");
        sb.AppendLine();
        sb.AppendLine("```json\n" + settings + "\n```");

        return sb.ToString();
    }

    public string IsValidCss(string css)
    {
        try
        {
            StylesheetParser parser = new StylesheetParser(true, true, false, false, false, false, false);
            parser.Parse(css);
            return "✅ valid css";
        }
        catch (Exception e)
        {
            return "❌ invalid css: " + e.Message;
        }
    }
}