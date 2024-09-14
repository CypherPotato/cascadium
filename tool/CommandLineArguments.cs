using Cascadium;
using Cascadium.Converters;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace cascadiumtool;

internal class CommandLineArguments
{
    internal bool IsCompiled = false;

    public List<string> InputFiles { get; set; } = new();
    public List<string> InputDirectories { get; set; } = new();
    public List<string> Extensions { get; set; } = new List<string>();
    public List<Regex> Exclude { get; set; } = new List<Regex>();
    public bool StdIn { get; set; } = false;
    public string? ConfigFile { get; set; }
    public string? OutputFile { get; set; }
    public bool Pretty { get; set; } = true;
    public bool UseVarShortcuts { get; set; } = true;
    public bool KeepNestingSpace { get; set; } = false;
    public MergeOption? MergeOption { get; set; }
    public MergeOrderPriority? MergeOrder { get; set; }
    public FilenameTagOption? FilenameTag { get; set; }
    public bool Watch { get; set; } = false;

    // only on json
    public List<StaticCSSConverter> Converters { get; set; } = new List<StaticCSSConverter>();
    public IDictionary<string, string> AtRuleRewriters { get; set; } = new Dictionary<string, string>();
}