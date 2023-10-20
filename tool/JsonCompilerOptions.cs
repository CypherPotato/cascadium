using CommandLine.Text;
using CommandLine;
using Cascadium.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Cascadium;
using System.Text.Json;
using System.IO;

namespace cascadiumtool;

[JsonSerializable(typeof(ICollection<string>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(StaticCSSConverter))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(JsonCssCompilerOptions))]
internal partial class JsonCssCompilerOptionsSerializerContext : JsonSerializerContext
{
}

internal class JsonCssCompilerOptions
{
    public ICollection<string> InputFiles { get; set; } = Array.Empty<string>();
    public ICollection<string> InputDirectories { get; set; } = Array.Empty<string>();
    public string? OutputFile { get; set; }
    public bool? KeepNestingSpace { get; set; }
    public bool? Pretty { get; set; }
    public bool? UseVarShortcut { get; set; }
    public bool? Merge { get; set; }

    public IEnumerable<StaticCSSConverter> Converters { get; set; } = Array.Empty<StaticCSSConverter>();
    public Dictionary<string, string> AtRulesRewrites { get; set; } = new Dictionary<string, string>();
    public IEnumerable<string> Extensions { get; set; } = Array.Empty<string>();
    public ICollection<string> ExcludePatterns { get; set; } = Array.Empty<string>();

    public static JsonCssCompilerOptions Create(string configFile)
    {
        string pathToConfigFile = PathUtils.ResolvePath(configFile);
        if (!File.Exists(pathToConfigFile))
        {
            Log.ErrorKill($"the specified config file was not found.");
            Environment.Exit(3);
        }

        string contents = File.ReadAllText(pathToConfigFile);

        JsonCssCompilerOptions? jsonConfig = JsonSerializer.Deserialize(
            contents,
            typeof(JsonCssCompilerOptions),
            new JsonCssCompilerOptionsSerializerContext(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            })) as JsonCssCompilerOptions;

        return jsonConfig!;
    }

    public void ApplyConfiguration(CascadiumOptions compilerOptions)
    {
        compilerOptions.Converters.AddRange(Converters);

        // options is instanted every time the compiler runs
        if (this.UseVarShortcut != null) compilerOptions.UseVarShortcut = this.UseVarShortcut.Value;
        if (this.Pretty != null) compilerOptions.Pretty = this.Pretty.Value;
        if (this.KeepNestingSpace != null) compilerOptions.KeepNestingSpace = this.KeepNestingSpace.Value;
        if (this.Merge != null) compilerOptions.Merge = this.Merge.Value;

        foreach (KeyValuePair<string, string> mediaRw in this.AtRulesRewrites)
        {
            compilerOptions.AtRulesRewrites.Add(mediaRw.Key, mediaRw.Value);
        }
    }
}
