using Cascadium;
using Cascadium.Converters;
using LightJson;
using LightJson.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace cascadiumtool;

public enum FilenameTagOption
{
    Full,
    Relative,
    None
}

internal class JsonCssCompilerOptions
{
    public static void Apply(string configFile, CommandLineArguments args)
    {
        if (!File.Exists(configFile))
        {
            Log.ErrorKill($"the specified config file was not found.");
            Environment.Exit(3);
        }

        JsonOptions.Default.PropertyNameCaseInsensitive = true;
        JsonOptions.Default.SerializationFlags = JsonSerializationFlags.Json5;
        JsonOptions.Default.Converters.Add(new StaticCSSConverterMapper());
        JsonOptions.Default.Converters.Add(new DictionaryMapper());

        JsonValue jsonObj = JsonReader.ParseFile(configFile);
        JsonCssCompilerOptions compilerConfig = new JsonCssCompilerOptions();

        if (jsonObj["InputFiles"].MaybeNull() is { } inputFiles)
            args.InputFiles.AddRange(inputFiles.GetJsonArray().Select(s => s.GetString()));
        if (jsonObj["InputDirectories"].MaybeNull() is { } inputDirectories)
            args.InputDirectories.AddRange(inputDirectories.GetJsonArray().Select(s => s.GetString()));
        if (jsonObj["ExcludePatterns"].MaybeNull() is { } ExcludePatterns)
            args.Exclude.AddRange(ExcludePatterns.GetJsonArray().Select(s => new Regex(s.GetString(), RegexOptions.IgnoreCase)));
        if (jsonObj["Extensions"].MaybeNull() is { } Extensions)
            args.Extensions.AddRange(Extensions.GetJsonArray().Select(s => s.GetString()));

        if (jsonObj["OutputFile"].MaybeNull() is { } OutputFile)
            args.OutputFile = OutputFile.GetString();

        if (jsonObj["Converters"].MaybeNull() is { } Converters)
            args.Converters.AddRange(Converters.GetJsonArray().Select(s => s.Get<StaticCSSConverter>()));
        if (jsonObj["AtRulesRewrites"].MaybeNull() is { } AtRulesRewrites)
            args.AtRuleRewriters = AtRulesRewrites.Get<IDictionary<string, string>>();

        if (jsonObj["Pretty"].MaybeNull() is { } Pretty)
            args.Pretty = Pretty.GetBoolean();
        if (jsonObj["UseVarShortcut"].MaybeNull() is { } UseVarShortcut)
            args.UseVarShortcuts = UseVarShortcut.GetBoolean();
        if (jsonObj["KeepNestingSpace"].MaybeNull() is { } KeepNestingSpace)
            args.KeepNestingSpace = KeepNestingSpace.GetBoolean();
        if (jsonObj["MergeOrderPriority"].MaybeNull() is { } MergeOrderPriority)
            args.MergeOrder = MergeOrderPriority.Get<MergeOrderPriority>();
        if (jsonObj["FilenameTag"].MaybeNull() is { } FilenameTag)
            args.FilenameTag = FilenameTag.Get<FilenameTagOption>();
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