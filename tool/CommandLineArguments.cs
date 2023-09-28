using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace tool;

internal class CommandLineArguments
{
    [Option('f', "file", HelpText = "Specifies a relative path to an input file.")]
    public ICollection<string> InputFiles { get; set; } = Array.Empty<string>();

    [Option('d', "dir", HelpText = "Specifies a relative path to recursively include an directory.")]
    public ICollection<string> InputDirectories { get; set; } = Array.Empty<string>();

    [Option('x', "extensions", HelpText = "Specify extensions (starting with dot) which the compiler will search for input directories.")]
    public ICollection<string> Extensions { get; set; } = Array.Empty<string>();

    [Option('o', "outfile", HelpText = "Specifies the output file where the compile CSS files will be written to.")]
    public string OutputFile { get; set; } = "";

    [Option("stdin", HelpText = "Specifies that the stdin should be included as an input.")]
    public bool StdIn { get; set; } = false;

    [Option("p:Pretty", Default = BoolType.True, HelpText = "Specifies whether the compiler should generate an pretty, indented and formatted code.")]
    public BoolType Pretty { get; set; }

    [Option("p:UseVarShortcut", Default = BoolType.True, HelpText = "Specifies whether the compiler should rewrite variable shortcuts.")]
    public BoolType UseVarShortcuts { get; set; }

    [Option("p:KeepNestingSpace", Default = BoolType.False, HelpText = "Specifies whether the compiler should keep spaces after the & operator.")]
    public BoolType KeepNestingSpace { get; set; }

    [Option("watch", HelpText = "Specifies if the compiler should watch for file changes and rebuild on each save.")]
    public bool Watch { get; set; } = false;
}

public enum BoolType
{
    False,
    True
}