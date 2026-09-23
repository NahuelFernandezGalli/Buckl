using System.Text.Json;
using Buckl.Api.Json;
using Buckl.Application.Common;

namespace Buckl.Api.Tests.Json;

public class FieldUpdateJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    [Fact]
    public void An_absent_property_leaves_the_field_untouched()
    {
        var body = JsonSerializer.Deserialize<NotesPatch>("{}", Options)!;

        Assert.False(body.Notes.IsSet);
    }

    [Fact]
    public void A_null_property_clears_the_field()
    {
        var body = JsonSerializer.Deserialize<NotesPatch>("""{ "notes": null }""", Options)!;

        Assert.True(body.Notes.IsSet);
        Assert.Null(body.Notes.Value);
    }

    [Fact]
    public void A_value_sets_the_field()
    {
        var body = JsonSerializer.Deserialize<NotesPatch>("""{ "notes": "Linen" }""", Options)!;

        Assert.True(body.Notes.IsSet);
        Assert.Equal("Linen", body.Notes.Value);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        BucklJson.Configure(options);

        return options;
    }
}

public sealed class NotesPatch
{
    public FieldUpdate<string?> Notes { get; init; }
}
