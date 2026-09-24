using System.Text.Json;
using System.Text.Json.Serialization;

namespace Buckl.Api.Json;

/// <summary>JSON conventions of the HTTP contract, applied to MVC and to the OpenAPI document:
/// enumerations as camel-case strings (<c>"top"</c>), never numbers, and partial-update fields
/// that tell an absent property from a null one.</summary>
public static class BucklJson
{
    public static void Configure(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        options.Converters.Add(new FieldUpdateJsonConverterFactory());
    }
}
