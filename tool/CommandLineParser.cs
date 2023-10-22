using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cascadiumtool;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal class OptionAttribute : Attribute
{
    public char? ShortVerb { get; set; }
    public string LongVerb { get; set; }
    public string? HelpText { get; set; }
    public string? Group { get; set; }
    public float Order { get; set; } = -1;

    public OptionAttribute(char shortTerm, string longTerm)
    {
        ShortVerb = shortTerm;
        LongVerb = longTerm;
    }

    public OptionAttribute(string longTerm)
    {
        LongVerb = longTerm;
    }
}

internal class CommandLineParser
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2072",
              Justification = "The return value of method 'System.Reflection.PropertyInfo.PropertyType.get' does not have matching annotations.")]
    public static bool TryParse
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>
    (string[] input, out TResult result, out Exception[] errorList)
    {
        Type tType = typeof(TResult);
        object res = Activator.CreateInstance<TResult>()!;
        List<Exception> exceptions = new List<Exception>();

        IEnumerable<OptionPair> options = GetOptionsFromType(tType);
        OptionPair? expectValue = null;
        for (int i = 0; i < input.Length; i++)
        {
            string current = input[i];
            string? next = i == input.Length - 1 ? null : input[i + 1];

            if (expectValue != null)
            {
                //  IL2072 : The return value of method 'System.Reflection.PropertyInfo.PropertyType.get' does not have matching annotations.
                AssociateValue(res, current, expectValue.Property, expectValue.Property.PropertyType);

                expectValue = null;
                continue;
            }

            OptionPair? pair;
            if (current.StartsWith("--"))
            {
                pair = options
                    .FirstOrDefault(o => string.Compare("--" + o.Option.LongVerb, current, true) == 0);
            }
            else if (current.StartsWith("-"))
            {
                pair = options
                    .FirstOrDefault(o => string.Compare("-" + o.Option.ShortVerb, current, true) == 0);
            }
            else
            {
                pair = null;
            }

            if (pair != null)
            {
                if (pair.Property.PropertyType == typeof(bool))
                {
                    // is switch
                    pair.Property.SetValue(res, true);
                }
                else
                {
                    expectValue = pair;
                }
            }
            else
            {
                exceptions.Add(new Exception("Unrecognized option term: " + current));
            }
        }

        result = (TResult)res;
        errorList = exceptions.ToArray();
        return exceptions.Count == 0;
    }

    private static void AssociateValue(
        object obj,
        string value,
        PropertyInfo prop,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] Type propType)
    {
        object castedValue;
        if (propType.IsEnum)
        {
            castedValue = Enum.Parse(propType, value, true);
        }
        else if (propType == typeof(ArrayList))
        {
            ArrayList collection = (ArrayList)prop.GetValue(obj)!;

            collection.Add(value);
            prop.SetValue(obj, collection);

            return;
        }
        else
        {
            castedValue = Convert.ChangeType(value, propType);
        }
        prop.SetValue(obj, castedValue);
    }

    private static IEnumerable<OptionPair> GetOptionsFromType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type t)
    {
        foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            OptionAttribute? opt = p.GetCustomAttribute<OptionAttribute>();
            if (opt != null)
                yield return new OptionPair(opt, p);
        }
    }

    public static void PrintHelp
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
    (string title, string? copyright, Exception[] errors)
    {
        Console.WriteLine(title);
        Console.WriteLine(copyright);
        Console.WriteLine();
        foreach (Exception er in errors)
        {
            Console.WriteLine("\t" + er.Message);
        }

        /// -------------------
        /// customization
        /// 
        int startTrailingSpace = 4;
        int optionGutter = 5;
        int helpTextMaxLength = 54;
        bool spaceBetweenOptions = true;
        /// 
        /// -------------------
        /// 
        int biggestOptionLength = 16;
        string lastGroup = "-";

        List<(string opText, string opHelp, string? opGroup, float order)> optionTexts = new();
        var options = GetOptionsFromType(typeof(T));
        foreach (var op in options)
        {
            if (op.Option.HelpText == null)
                continue;

            string ops = op.Option.ShortVerb != null ?
                "-" + op.Option.ShortVerb + ", --" + op.Option.LongVerb
                : "    --" + op.Option.LongVerb;

            biggestOptionLength = Math.Max(biggestOptionLength, ops.Length);

            float o = op.Option.Order;
            if (op.Option.ShortVerb == null)
                o += 0.1f;

            optionTexts.Add((ops.ToLower(), op.Option.HelpText, op.Option.Group, o));
        }

        foreach (var opKp in optionTexts.OrderBy(o => o.order))
        {
            if (opKp.opGroup != null && lastGroup != opKp.opGroup)
            {
                Console.WriteLine("\n" + opKp.opGroup + ":\n");
                lastGroup = opKp.opGroup;
            }

            string[] parts = GetWordGroups(opKp.opHelp, helpTextMaxLength).ToArray();
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (i == 0)
                {
                    Console.WriteLine("{0," + startTrailingSpace + "}{1,-" + (biggestOptionLength + optionGutter) + "}{2}", "", opKp.opText, part);
                }
                else
                {
                    Console.WriteLine("{0}{1}", new String(' ', biggestOptionLength + optionGutter + startTrailingSpace), part);
                }
            }

            if (spaceBetweenOptions)
                Console.WriteLine();
        }
    }

    private static List<string> GetWordGroups(string text, int limit)
    {
        var words = text.Split(new string[] { " ", "\r\n", "\n" }, StringSplitOptions.None);

        List<string> wordList = new List<string>();

        string line = "";
        foreach (string word in words)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                var newLine = string.Join(" ", line, word).Trim();
                if (newLine.Length >= limit)
                {
                    wordList.Add(line);
                    line = word;
                }
                else
                {
                    line = newLine;
                }
            }
        }

        if (line.Length > 0)
            wordList.Add(line);

        return wordList;
    }

    private class OptionPair
    {
        public OptionAttribute Option { get; set; }
        public PropertyInfo Property { get; set; }

        public OptionPair(OptionAttribute option, PropertyInfo property)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            Property = property ?? throw new ArgumentNullException(nameof(property));
        }
    }
}
