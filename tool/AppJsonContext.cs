using Cascadium.Converters;
using LightJson;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace cascadiumtool;

[JsonSerializable(typeof(CascadiumCompilationConfiguration))]
public partial class AppJsonContext : JsonSerializerContext
{
}


public class StaticCSSConverterMapper : LightJson.Converters.JsonConverter
{
    public override Boolean CanSerialize(Type obj, JsonOptions options)
    {
        return obj == typeof(StaticCSSConverter);
    }

    public override Object Deserialize(JsonValue value, Type requestedType, JsonOptions options)
    {
        return new StaticCSSConverter()
        {
            ArgumentCount = value["ArgumentCount"].MaybeNull()?.GetInteger(),
            MatchProperty = value["MatchProperty"].GetString(),
            Output = value["Output"].Get<IDictionary<string, string>>()
        };
    }

    public override JsonValue Serialize(Object value, JsonOptions options)
    {
        throw new NotImplementedException();
    }
}