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

    [Option("no-pretty", HelpText = "Specifies if the output should NOT generate an pretty, indented and formatted code.")]
    public bool NoPretty { get; set; } = false;

    [Option("no-varshortcuts", HelpText = "Specifies if the output should NOT rewrite variable shortcuts.")]
    public bool NoUseVarShortcuts { get; set; } = false;

    [Option("watch", HelpText = "Specifies if the compiler should watch for file changes and rebuild on each save.")]
    public bool Watch { get; set; } = false;
}