using System;
using System.Collections;
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

    [Option('f', "file", Group = "Input options", Order = 1, HelpText = "Specifies a relative path to an input file.")]
    public ArrayList InputFiles { get; set; } = new ArrayList();

    [Option('d', "dir", Group = "Input options", Order = 1, HelpText = "Specifies a relative path to recursively include an directory.")]
    public ArrayList InputDirectories { get; set; } = new ArrayList();

    [Option('x', "extensions", Group = "Input options", Order = 1, HelpText = "Specify extensions (starting with dot) which the compiler will search for input directories.")]
    public ArrayList Extensions { get; set; } = new ArrayList();

    [Option('e', "exclude", Group = "Input options", Order = 1, HelpText = "Exclude an file or directory that matches the specified regex.")]
    public ArrayList Exclude { get; set; } = new ArrayList();

    [Option("stdin", Group = "Input options", Order = 1, HelpText = "Specifies that the stdin should be included as an input.")]
    public bool StdIn { get; set; } = false;

    [Option('c', "config", Group = "Input options", Order = 1, HelpText = "Specifies the relative or absolute path to the configuration file.")]
    public string? ConfigFile { get; set; }

    [Option('o', "outfile", Group = "Output options", Order = 2, HelpText = "Specifies the output file where the compile CSS files will be written to.")]
    public string OutputFile { get; set; } = "";

    [Option("p:Pretty", Group = "Compiler settings", Order = 3, HelpText = "Specifies whether the compiler should generate an pretty, indented and formatted code.")]
    public BoolType Pretty { get; set; } = BoolType.True;

    [Option("p:UseVarShortcut", Group = "Compiler settings", Order = 3, HelpText = "Specifies whether the compiler should rewrite variable shortcuts.")]
    public BoolType UseVarShortcuts { get; set; } = BoolType.True;

    [Option("p:KeepNestingSpace", Group = "Compiler settings", Order = 3, HelpText = "Specifies whether the compiler should keep spaces after the & operator.")]
    public BoolType KeepNestingSpace { get; set; } = BoolType.False;

    [Option("p:Merge", Group = "Compiler settings", Order = 3, HelpText = "Specifies whether the compiler should merge rules and at-rules.")]
    public BoolType Merge { get; set; } = BoolType.False;

    [Option("watch", Group = "Other", Order = 4, HelpText = "Specifies if the compiler should watch for file changes and rebuild on each save.")]
    public bool Watch { get; set; } = false;

    public void Import(JsonCssCompilerOptions? jsonConfig)
    {
        if (!IsCompiled)
        {
            InputDirectories = new ArrayList(InputDirectories);
            InputFiles = new ArrayList(InputFiles);
            Extensions = new ArrayList(Extensions);
            Exclude = new ArrayList(Exclude);

            if (jsonConfig != null)
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