using System.Text.Json;
using System.Text.Json.Serialization;
using Buckl.Application.Common;

namespace Buckl.Api.Json;

/// <summary>Reads a <see cref="FieldUpdate{T}"/>: an absent property never reaches the
/// converter and stays unset; a present one, including <c>null</c>, becomes a set
/// value.</summary>
public sealed class FieldUpdateJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(FieldUpdate<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        var converterType = typeof(FieldUpdateJsonConverter<>)
            .MakeGenericType(typeToConvert.GetGenericArguments()[0]);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class FieldUpdateJsonConverter<T> : JsonConverter<FieldUpdate<T>>
{
    /// <summary>Null tokens must reach <see cref="Read"/>: they mean "clear the field".</summary>
    public override bool HandleNull => true;

    public override FieldUpdate<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        new(JsonSerializer.Deserialize<T>(ref reader, options)!);

    public override void Write(
        Utf8JsonWriter writer,
        FieldUpdate<T> value,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.Value, options);
}
