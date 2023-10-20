using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace cascadiumtool;

internal class Program
{
    public const string VersionLabel = "v.0.1.1-alpha";
    public static string CurrentDirectory { get; } = Directory.GetCurrentDirectory();
    public static bool HasRootConfiguration { get; private set; }
    public static JsonCssCompilerOptions? CompilerOptions { get; set; }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CommandLineArguments))]
    static int Main(string[] args)
    {
        CommandLineParser.TryParse<CommandLineArguments>(args, out var result, out var errors);

        if (errors.Length > 0 || args.Length == 0)
        {
            CommandLineParser.PrintHelp<CommandLineArguments>($"Cascadium [{VersionLabel}]", "Distributed under MIT License", errors);
            return 0;
        } else
        {
            RunParsed(result, out int errorcode);
            return errorcode;
        }
    }

    public static void RunParsed(CommandLineArguments args, out int errorcode)
    {
        if (args.ConfigFile != null)
        {
            CompilerOptions = JsonCssCompilerOptions.Create(args.ConfigFile);
        }

        args.Import(CompilerOptions);

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
