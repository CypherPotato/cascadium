using CommandLine;
using CommandLine.Text;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace tool;

internal class Program
{
    public static string CurrentDirectory { get; } = Directory.GetCurrentDirectory();
    public static bool HasRootConfiguration { get; private set; }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CommandLineArguments))]
    static int Main(string[] args)
    {
        var parser = new Parser(with =>
        {
            with.AutoHelp = true;
            with.AutoVersion = true;
            with.AllowMultiInstance = true;
            with.CaseInsensitiveEnumValues = true;
            with.MaximumDisplayWidth = Console.BufferWidth;
            with.IgnoreUnknownArguments = false;
        });

        int errorcode = 0;
        var result = parser.ParseArguments<CommandLineArguments>(args);
        
        if(args.Length == 0)
        {
            Console.WriteLine(HelpText.AutoBuild(result, _ => _, _ => _));
            return -1;
        }

        result
            .WithParsed(args => RunParsed(args, out errorcode))
            .WithNotParsed(err =>
            {
                ShowHelp(result);
            });

        return errorcode;
    }

    public static void ShowHelp(ParserResult<CommandLineArguments> pResult)
    {
        var helpText = HelpText.AutoBuild(pResult, h =>
        {
            h.AutoVersion = true;
            h.Heading = "Simple CSS Compiler - XCSS";
            h.Copyright = "distributed under MIT license";
            h.AddEnumValuesToHelpText = true;

            return h;
        });
        Console.WriteLine(helpText);
    }

    public static void RunParsed(CommandLineArguments args, out int errorcode)
    {
        if (args.Watch)
        {
            errorcode = Watcher.Watch(args);
        }
        else
        {
            errorcode = Compiler.RunCompiler(args);
        }
    }
}
