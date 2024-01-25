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

        JsonOptions.PropertyNameCaseInsensitive = true;
        JsonOptions.Mappers.Add(new StaticCSSConverterMapper());
        JsonOptions.Mappers.Add(new DictionaryMapper());

        JsonObject jsonObj = JsonValue.Parse(contents).GetJsonObject();
        JsonCssCompilerOptions compilerConfig = new JsonCssCompilerOptions();

        compilerConfig.InputFiles = new List<string>(jsonObj["InputFiles"].MaybeNull()?.GetJsonArray().Select(i => i.GetString()) ?? Array.Empty<string>());
        compilerConfig.InputDirectories = new List<string>(jsonObj["InputDirectories"].MaybeNull()?.GetJsonArray().Select(i => i.GetString()) ?? Array.Empty<string>());
        compilerConfig.OutputFile = jsonObj["OutputFile"].MaybeNull()?.GetString();
        compilerConfig.KeepNestingSpace = jsonObj["KeepNestingSpace"].MaybeNull()?.GetBoolean();
        compilerConfig.Pretty = jsonObj["pretty"].MaybeNull()?.GetBoolean();
        compilerConfig.UseVarShortcut = jsonObj["useVarShortcut"].MaybeNull()?.GetBoolean();
        compilerConfig.Converters = jsonObj["Converters"].MaybeNull()?.GetJsonArray().Select(s => s.Get<StaticCSSConverter>()) ?? Array.Empty<StaticCSSConverter>();
        compilerConfig.AtRulesRewrites = jsonObj["AtRulesRewrites"].MaybeNull()?.Get<IDictionary<string, string>>() ?? new Dictionary<string, string>();
        compilerConfig.Extensions = jsonObj["Extensions"].MaybeNull()?.GetJsonArray().Select(s => s.GetString()) ?? Array.Empty<string>();
        compilerConfig.ExcludePatterns = jsonObj["ExcludePatterns"].MaybeNull()?.GetJsonArray().Select(s => s.GetString()) ?? Array.Empty<string>();

        return compilerConfig;
    }

    public void ApplyConfiguration(CascadiumOptions compilerOptions)
    {
        compilerOptions.Converters.AddRange(Converters);

        // options is instanted every time the compiler runs
        if (this.UseVarShortcut != null) compilerOptions.UseVarShortcut = this.UseVarShortcut.Value;
        if (this.Pretty != null) compilerOptions.Pretty = this.Pretty.Value;
        if (this.KeepNestingSpace != null) compilerOptions.KeepNestingSpace = this.KeepNestingSpace.Value;

        foreach (KeyValuePair<string, string> mediaRw in this.AtRulesRewrites)
        {
            compilerOptions.AtRulesRewrites.Add(mediaRw.Key, mediaRw.Value);
        }
    }
}

public class DictionaryMapper : LightJson.Converters.JsonConverter
{
    public override Boolean CanSerialize(Type obj)
    {
        return obj == typeof(IDictionary<string, string>);
    }

    public override Object Deserialize(JsonValue value, Type requestedType)
    {
        var dict = new Dictionary<string, string>();

        var obj = value.GetJsonObject();

        foreach (var kvp in obj.Properties)
        {
            dict.Add(kvp.Key, kvp.Value.GetString());
        }

        return dict;
    }

    public override JsonValue Serialize(Object value)
    {
        throw new NotImplementedException();
    }
}

public class StaticCSSConverterMapper : LightJson.Converters.JsonConverter
{
    public override Boolean CanSerialize(Type obj)
    {
        return obj == typeof(StaticCSSConverter);
    }

    public override Object Deserialize(JsonValue value, Type requestedType)
    {
        return new StaticCSSConverter()
        {
            ArgumentCount = value["ArgumentCount"].MaybeNull()?.GetInteger(),
            MatchProperty = value["MatchProperty"].GetString(),
            Output = value["Output"].Get<IDictionary<string, string>>()
        };
    }

    public override JsonValue Serialize(Object value)
    {
        throw new NotImplementedException();
    }
}