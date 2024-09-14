using Cascadium;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cascadiumtool;

internal class Program
{
    public const string VersionLabel = "v.0.8.0";
    public static string CurrentDirectory { get; set; } = Directory.GetCurrentDirectory();
    public static Dictionary<string, string> CompilerCache { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static string[] Greetings = [
        "Good morning, sunshine Cascadium!",
        "Cascadium is heating up. Please, wait.",
        "Hello, Cascadium!",
        "Starting the Cascadium engines",
        "Is the coffee ready? Let's code!",
        "Hey hey, Cascadium!",
        "Cascadium is excited today! Are you?",
        "Another day, more cascading style sheets!",
        "Cascadium is saying hello to you. Say it back!",
        "Looks like it's the Cascadium code-hour!",
        "Wake up, Cascadium! The programmer wants you to do things again."
    ];

    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 0;
        }

        CommandLineArguments arguments = new CommandLineArguments();
        var parsed = new CommandLineParser(args.Skip(1).ToArray());

        string[] implicitConfigFiles = [
            "cssconfig.json",
            "cssconfig.json5",
            "cascadium.json",
            "cascadium.json5"
        ];

        string? configFile = null;
        arguments.ConfigFile = parsed.GetValue("config", 'c');

        if (arguments.ConfigFile is not null)
        {
            configFile = PathUtils.ResolvePath(arguments.ConfigFile);
        }
        else foreach (string implicitFile in implicitConfigFiles)
            {
                string rpath = PathUtils.ResolvePath(implicitFile);
                if (File.Exists(rpath))
                {
                    configFile = rpath;
                    break;
                }
            }

        string runVerb = args[0];
        if (string.Compare(runVerb, "watch", true) == 0)
        {
            arguments.Watch = true;
            Log.Info(Greetings[Random.Shared.Next(0, Greetings.Length - 1)]);
            Log.Info("Caching the current XCSS repository...");
        }

        // options that are lists. json should add to them
        arguments.InputFiles = parsed.GetValues("file", 'f').ToList();
        arguments.Extensions = parsed.GetValues("extension", 'x').ToList();
        arguments.Exclude = parsed.GetValues("exclude", 'e').Select(x => new Regex(x, RegexOptions.IgnoreCase)).ToList();
        arguments.InputDirectories = parsed.GetValues("dir", 'd').ToList();

        // options that arent present on config json
        arguments.StdIn = parsed.IsDefined("stdin");

        // options that its priority is above config json
        if (parsed.GetValue("outfile", 'o') is { } outfile)
            arguments.OutputFile = outfile;

        if (parsed.GetValue("p:merge") is { } pmerge)
            arguments.MergeOption = Enum.Parse<MergeOption>(pmerge, true);

        if (parsed.GetValue("p:mergeorder") is { } pmergeorder)
            arguments.MergeOrder = Enum.Parse<MergeOrderPriority>(pmergeorder, true);

        if (parsed.GetValue("p:pretty") is { } ppretty)
            arguments.Pretty = ppretty == "true";

        if (parsed.GetValue("p:keepnestingspace") is { } pkeepnestingspace)
            arguments.KeepNestingSpace = pkeepnestingspace == "true";

        if (parsed.GetValue("p:usevarshortcuts") is { } pusevarshortcuts)
            arguments.UseVarShortcuts = pusevarshortcuts == "true";

        if (parsed.GetValue("p:filenametag") is { } pfilenametag)
            arguments.FilenameTag = Enum.Parse<FilenameTagOption>(pfilenametag, true);

        if (configFile is not null)
        {
            JsonCssCompilerOptions.Apply(configFile, arguments);
            CurrentDirectory = Path.GetDirectoryName(configFile)!;
        }

        return await RunParsed(arguments);
    }

    public static async Task<int> RunParsed(CommandLineArguments args)
    {
        if (args.Watch)
        {
            return await Watcher.Watch(args);
        }
        else
        {
            return await Compiler.RunCompiler(args);
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine($"Cascadium Tool [{VersionLabel}]");
        Console.WriteLine($"Distributed under MIT License");
        Console.WriteLine($"Visit Cascadium at https://github.com/CypherPotato/cascadium");
        Console.WriteLine();
        Console.WriteLine($"Usage: CASCADIUM <watch|build> [...options]");
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

            Compiler options:

                Pretty: sets whether the compiler should generate an pretty, indented and formatted output.

                    --p:pretty <true/false>

                UseVarShortcuts: sets whether the compiler should rewrite var() shortcuts.

                    --p:usevarshortcuts <true/false>

                KeepNestingSpace: sets whether the compiler should keep spaces between '&' and selectors.

                    --p:keepnestingspace <true/false>

                Merge: sets how the compiler should merge code.

                    --p:merge <none|selectors|atrules|declarations|all>

                MergeOrder: specify the merging order position. Merge is not available in the watch mode.
            
                    --p:merge <preservefirst|preservelast>

                FilenameTag: specifies whether the compiler should include a compiled file name tag in the
                             compiler output.

                    --p:filenametag <full|relative|none>
            """);
    }
}
