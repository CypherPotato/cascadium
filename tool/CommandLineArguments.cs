using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cascadiumtool;

internal class CommandLineArguments
{
    internal bool IsCompiled = false;
    internal List<Regex> CompiledExcludes = new List<Regex>();

    [Option('f', "file", HelpText = "Specifies a relative path to an input file.")]
    public ICollection<string> InputFiles { get; set; } = Array.Empty<string>();

    [Option('d', "dir", HelpText = "Specifies a relative path to recursively include an directory.")]
    public ICollection<string> InputDirectories { get; set; } = Array.Empty<string>();

    [Option('x', "extensions", HelpText = "Specify extensions (starting with dot) which the compiler will search for input directories.")]
    public ICollection<string> Extensions { get; set; } = Array.Empty<string>();

    [Option('e', "exclude", HelpText = "Exclude an file or directory that matches the specified regex.")]
    public ICollection<string> Exclude { get; set; } = Array.Empty<string>();

    [Option('o', "outfile", HelpText = "Specifies the output file where the compile CSS files will be written to.")]
    public string OutputFile { get; set; } = "";

    [Option('c', "config", HelpText = "Specifies the relative or absolute path to the configuration file.")]
    public string? ConfigFile { get; set; }

    [Option("stdin", HelpText = "Specifies that the stdin should be included as an input.")]
    public bool StdIn { get; set; } = false;

    [Option("p:Pretty", Default = BoolType.True, HelpText = "Specifies whether the compiler should generate an pretty, indented and formatted code.")]
    public BoolType Pretty { get; set; }

    [Option("p:UseVarShortcut", Default = BoolType.True, HelpText = "Specifies whether the compiler should rewrite variable shortcuts.")]
    public BoolType UseVarShortcuts { get; set; }

    [Option("p:KeepNestingSpace", Default = BoolType.False, HelpText = "Specifies whether the compiler should keep spaces after the & operator.")]
    public BoolType KeepNestingSpace { get; set; }

    [Option("p:Merge", Default = BoolType.False, HelpText = "Specifies whether the compiler should merge rules and at-rules.")]
    public BoolType Merge { get; set; }

    [Option("watch", HelpText = "Specifies if the compiler should watch for file changes and rebuild on each save.")]
    public bool Watch { get; set; } = false;

    public void Import(JsonCssCompilerOptions? jsonConfig)
    {
        if (!IsCompiled)
        {
            InputDirectories = new List<string>(InputDirectories);
            InputFiles = new List<string>(InputFiles);
            Extensions = new List<string>(Extensions);
            Exclude = new List<string>(Exclude);

            if(jsonConfig != null)
            {
                foreach (var file in jsonConfig.InputFiles) InputFiles.Add(file);
                foreach (var dir in jsonConfig.InputDirectories) InputDirectories.Add(dir);
                foreach (var ext in jsonConfig.Extensions) Extensions.Add(ext);
                foreach (var exc in jsonConfig.ExcludePatterns) Exclude.Add(exc);
                if (jsonConfig.OutputFile != null) OutputFile = jsonConfig.OutputFile;
            }

            foreach (string exPattern in Exclude)
            {
                try
                {
                    CompiledExcludes.Add(new Regex(exPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                catch (Exception ex)
                {
                    Log.ErrorKill("Couldn't parse the exclude regex " + exPattern + ": " + ex.Message);
                }
            }

            IsCompiled = true;
        }
    }
}

public enum BoolType
{
    False,
    True
}