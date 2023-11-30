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
using System.Diagnostics.CodeAnalysis;
using LightJson;
using System.Collections;

namespace cascadiumtool;

internal class JsonCssCompilerOptions
{
    public ICollection<string> InputFiles { get; set; } = Array.Empty<string>();
    public ICollection<string> InputDirectories { get; set; } = Array.Empty<string>();
    public string? OutputFile { get; set; }
    public bool? KeepNestingSpace { get; set; }
    public bool? Pretty { get; set; }
    public bool? UseVarShortcut { get; set; }
    public string? Merge { get; set; }
    public string? MergeOrderPriority { get; set; }

    public IEnumerable<StaticCSSConverter> Converters { get; set; } = Array.Empty<StaticCSSConverter>();
    public IDictionary<string, string> AtRulesRewrites { get; set; } = new Dictionary<string, string>();
    public IEnumerable<string> Extensions { get; set; } = Array.Empty<string>();
    public IEnumerable<string> ExcludePatterns { get; set; } = Array.Empty<string>();

    public static JsonCssCompilerOptions Create(string configFile)
    {
        string pathToConfigFile = PathUtils.ResolvePath(configFile);
        if (!File.Exists(pathToConfigFile))
        {
            Log.ErrorKill($"the specified config file was not found.");
            Environment.Exit(3);
        }

        string contents = File.ReadAllText(pathToConfigFile);

        JsonOptions.ThrowOnInvalidCast = true;
        JsonOptions.PropertyNameCaseInsensitive = true;
        JsonOptions.Mappers.Add(new StaticCSSConverterMapper());
        JsonOptions.Mappers.Add(new DictionaryMapper());

        JsonObject jsonObj = JsonValue.Parse(contents).AsJsonObject!;
        JsonCssCompilerOptions compilerConfig = new JsonCssCompilerOptions();

        compilerConfig.InputFiles = new List<string>(jsonObj["InputFiles"].MaybeNull()?.AsJsonArray!.Select(i => i.AsString!) ?? Array.Empty<string>());
        compilerConfig.InputDirectories = new List<string>(jsonObj["InputDirectories"].MaybeNull()?.AsJsonArray!.Select(i => i.AsString!) ?? Array.Empty<string>());
        compilerConfig.OutputFile = jsonObj["OutputFile"].MaybeNull()?.AsString;
        compilerConfig.KeepNestingSpace = jsonObj["KeepNestingSpace"].MaybeNull()?.AsBoolean;
        compilerConfig.Pretty = jsonObj["pretty"].MaybeNull()?.AsBoolean;
        compilerConfig.UseVarShortcut = jsonObj["useVarShortcut"].MaybeNull()?.AsBoolean;
        compilerConfig.Merge = jsonObj["merge"].MaybeNull()?.AsString;
        compilerConfig.MergeOrderPriority = jsonObj["MergeOrderPriority"].MaybeNull()?.AsString;
        compilerConfig.Converters = jsonObj["Converters"].MaybeNull()?.AsJsonArray!.EveryAs<StaticCSSConverter>() ?? Array.Empty<StaticCSSConverter>();
        compilerConfig.AtRulesRewrites = jsonObj["AtRulesRewrites"].MaybeNull()?.As<IDictionary<string, string>>() ?? new Dictionary<string, string>();
        compilerConfig.Extensions = jsonObj["Extensions"].MaybeNull()?.AsJsonArray!.Select(s => s.AsString!) ?? Array.Empty<string>();
        compilerConfig.ExcludePatterns = jsonObj["ExcludePatterns"].MaybeNull()?.AsJsonArray!.Select(s => s.AsString!) ?? Array.Empty<string>();

        return compilerConfig;
    }

    [DynamicDependency("MergeOption")]
    [DynamicDependency("MergeOrderPriority")]
    public void ApplyConfiguration(CascadiumOptions compilerOptions)
    {
        compilerOptions.Converters.AddRange(Converters);

        // options is instanted every time the compiler runs
        if (this.UseVarShortcut != null) compilerOptions.UseVarShortcut = this.UseVarShortcut.Value;
        if (this.Pretty != null) compilerOptions.Pretty = this.Pretty.Value;
        if (this.KeepNestingSpace != null) compilerOptions.KeepNestingSpace = this.KeepNestingSpace.Value;
        if (this.Merge != null) compilerOptions.Merge = Enum.Parse<MergeOption>(this.Merge, true);
        if (this.MergeOrderPriority != null) compilerOptions.MergeOrderPriority = Enum.Parse<MergeOrderPriority>(this.MergeOrderPriority, true);

        foreach (KeyValuePair<string, string> mediaRw in this.AtRulesRewrites)
        {
            compilerOptions.AtRulesRewrites.Add(mediaRw.Key, mediaRw.Value);
        }
    }
}

public class DictionaryMapper : JsonSerializerMapper
{
    public override Boolean CanSerialize(Type obj)
    {
        return obj == typeof(IDictionary<string, string>);
    }

    public override Object Deserialize(JsonValue value)
    {
        var dict = new Dictionary<string, string>();

        value.EnsureType(JsonValueType.Object);
        var obj = value.AsJsonObject!;

        foreach (var kvp in obj.Properties)
        {
            dict.Add(kvp.Key, kvp.Value.AsString!);
        }

        return dict;
    }

    public override JsonValue Serialize(Object value)
    {
        throw new NotImplementedException();
    }
}

public class StaticCSSConverterMapper : JsonSerializerMapper
{
    public override Boolean CanSerialize(Type obj)
    {
        return obj == typeof(StaticCSSConverter);
    }

    public override Object Deserialize(JsonValue value)
    {
        return new StaticCSSConverter()
        {
            ArgumentCount = (int)value["ArgumentCount"].AsNumber,
            MatchProperty = value["MatchProperty"].AsString,
            Output = value["Output"].As<IDictionary<string, string>>()
        };
    }

    public override JsonValue Serialize(Object value)
    {
        throw new NotImplementedException();
    }
}