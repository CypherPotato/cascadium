using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace cascadiumtool;

internal class Program
{
    public const string VersionLabel = "v.0.5";
    public static string CurrentDirectory { get; set; } = Directory.GetCurrentDirectory();
    public static bool HasRootConfiguration { get; private set; }
    public static JsonCssCompilerOptions? CompilerOptions { get; set; }
    public static Dictionary<string, string> CompilerCache { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public static bool IsWatch { get; set; } = false;

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CommandLineArguments))]
    static async Task<int> Main(string[] args)
    {
        CommandLineParser.TryParse<CommandLineArguments>(args, out var result, out var errors);

        if (errors.Length > 0 || args.Length == 0)
        {
            CommandLineParser.PrintHelp<CommandLineArguments>($"Cascadium [{VersionLabel}]", "Distributed under MIT License", errors);
            return 0;
        }
        else
        {
            return await RunParsed(result);
        }
    }

    public static async Task<int> RunParsed(CommandLineArguments args)
    {
        if (args.ConfigFile != null)
        {
            string fullPath = PathUtils.ResolvePath(args.ConfigFile);
            CompilerOptions = JsonCssCompilerOptions.Create(fullPath);
            Program.CurrentDirectory = Path.GetDirectoryName(fullPath)!;
        }

        args.Import(CompilerOptions);

        if (args.Watch)
        {
            IsWatch = true;
            return await Watcher.Watch(args);
        }
        else
        {
            return await Compiler.RunCompiler(args);
        }
    }
}
