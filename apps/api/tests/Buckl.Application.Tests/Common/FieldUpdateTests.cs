using Buckl.Application.Common;

namespace Buckl.Application.Tests.Common;

public class FieldUpdateTests
{
    [Fact]
    public void The_default_value_leaves_the_field_untouched()
    {
        FieldUpdate<string?> update = default;

        Assert.False(update.IsSet);
    }

    [Fact]
    public void A_null_value_is_an_explicit_request_to_clear_the_field()
    {
        var update = new FieldUpdate<string?>(null);

        Assert.True(update.IsSet);
        Assert.Null(update.Value);
    }

    [Fact]
    public void A_value_is_carried_as_given()
    {
        var update = new FieldUpdate<string?>("Linen");

        Assert.True(update.IsSet);
        Assert.Equal("Linen", update.Value);
    }
}
