using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace cascadiumtool;

internal class Program
{
    public const string VersionLabel = "v.0.7.0";
    public static string CurrentDirectory { get; set; } = Directory.GetCurrentDirectory();
    public static bool HasRootConfiguration { get; private set; }
    public static JsonCssCompilerOptions? CompilerOptions { get; set; }
    public static Dictionary<string, string> CompilerCache { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public static bool IsWatch { get; set; } = false;

    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            //CommandLineParser.PrintHelp<CommandLineArguments>($"Cascadium [{VersionLabel}]", "Distributed under MIT License", errors);
            return 0;
        }

        CommandLineArguments arguments = new CommandLineArguments();
        var parsed = new CommandLineParser(args);

        arguments.ConfigFile = parsed.GetValue("config", 'c');

        // options that are lists. json should add to them
        arguments.InputFiles = parsed.GetValues("file", 'f').ToList();
        arguments.Extensions = parsed.GetValues("extension", 'x').ToList();
        arguments.Exclude = parsed.GetValues("exclude", 'e').ToList();
        arguments.InputDirectories = parsed.GetValues("dir", 'd').ToList();

        if (arguments.ConfigFile != null)
        {
            string fullPath = PathUtils.ResolvePath(arguments.ConfigFile);
            CompilerOptions = JsonCssCompilerOptions.Create(fullPath);
            Program.CurrentDirectory = Path.GetDirectoryName(fullPath)!;

            arguments.Import(CompilerOptions);
        }

        // options that arent present on config json
        arguments.Watch = parsed.IsDefined("watch");
        arguments.StdIn = parsed.IsDefined("stdin");

        // options that its priority is above config json
        if (parsed.GetValue("outfile", 'o') is { } outfile)
            arguments.OutputFile = outfile;

        if (parsed.GetValue("p:merge") is { } pmerge)
            arguments.MergeOption = pmerge;

        if (parsed.GetValue("p:mergeorder") is { } pmergeorder)
            arguments.MergeOrder = pmergeorder;

        if (parsed.GetValue("p:pretty") is { } ppretty)
            arguments.Pretty = ppretty == "true";

        if (parsed.GetValue("p:keepnestingspace") is { } pkeepnestingspace)
            arguments.KeepNestingSpace = pkeepnestingspace == "true";

        if (parsed.GetValue("p:usevarshortcuts") is { } pusevarshortcuts)
            arguments.UseVarShortcuts = pusevarshortcuts == "true";

        return await RunParsed(arguments);
    }

    public static async Task<int> RunParsed(CommandLineArguments args)
    {
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

    void PrintHelp()
    {
        Console.WriteLine($"Cascadium Tool [{VersionLabel}]");
        Console.WriteLine($"Distributed under MIT License");
        Console.WriteLine($"Visit Cascadium at https://github.com/CypherPotato/cascadium");
        Console.WriteLine();
        Console.WriteLine($"Usage: CASCADIUM [...options]");
        Console.WriteLine();
        Console.WriteLine("""

            Input options:

                -f, --file          Specifies a relative path to an input file.

                -d, --dir           Specifies a relative path to recursively include an
                                    directory contents and sub contents.

                -x, --extensions    Specify extensions (starting with dot) which the compiler will
                                    include from directories. By default, the compiler only searches for
                                    .xcss and .css files.

                -e, --exclude       Specifies an exclude regex pattern to exclude files. The regex is
                                    tested against the absolute file path.

                -c, --config        Specifies a relative path to the configuration file.

                    --stdin         Includes the process stdin into the compiler.

            Output options:

                -o, --outfile       Specifies the relative output file where the compiled CSS file should be
                                    written to.

            Development options:

                    --watch         Specifies that the compiler should watch for file changes and rebuild on
                                    each change.

            Compiler options:

                Pretty: sets whether the compiler should generate an pretty, indented and formatted
                output.

                    --p:pretty <true/false>

                UseVarShortcuts: sets whether the compiler should rewrite var() shortcuts.

                    --p:usevarshortcuts <true/false>

                KeepNestingSpace: sets whether the compiler should keep spaces between '&' and selectors.

                    --p:keepnestingspace <true/false>

                Merge: sets how the compiler should merge code.

                    --p:merge <none|selectors|atrules|declarations|all>

                MergeOrder: specify the merging order position.
            
                    --p:merge <preservefirst|preservelast>
            """);
    }
}
