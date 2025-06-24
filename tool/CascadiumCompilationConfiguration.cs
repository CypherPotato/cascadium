using Cascadium;
using Cascadium.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace cascadiumtool;

public sealed class CascadiumCompilationConfiguration
{
    public List<string> InputFiles { get; set; } = [];
    public List<string> InputDirectories { get; set; } = [];
    public List<string> Extensions { get; set; } = [];
    public List<Regex> Exclude { get; set; } = [];

    [JsonIgnore]
    public bool StdIn { get; set; } = false;

    [JsonIgnore]
    public string? ConfigFile { get; set; }

    [JsonPropertyName("outFile")]
    public string? OutputFile { get; set; }
    public bool Pretty { get; set; } = true;

    [JsonPropertyName("UseVarShortcut")]
    public bool UseVarShortcuts { get; set; } = true;
    public bool KeepNestingSpace { get; set; } = false;
    public MergeOption MergeOption { get; set; } = MergeOption.None;
    public MergeOrderPriority MergeOrder { get; set; } = MergeOrderPriority.PreserveLast;
    public FilenameTagOption FilenameTag { get; set; } = FilenameTagOption.None;

    [JsonIgnore]
    public bool Watch { get; set; } = false;
    public List<StaticCSSConverter> Converters { get; set; } = [];
    public IDictionary<string, string> AtRuleRewriters { get; set; } = new Dictionary<string, string>();
}

public enum FilenameTagOption
{
    Full,
    Relative,
    None
}