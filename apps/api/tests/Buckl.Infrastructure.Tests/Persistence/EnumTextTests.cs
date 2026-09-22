using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Infrastructure.Persistence;

namespace Buckl.Infrastructure.Tests.Persistence;

public class EnumTextTests
{
    [Fact]
    public void ToText_writes_the_enum_name_in_lower_case()
    {
        Assert.Equal("outerwear", EnumText.ToText(Category.Outerwear));
        Assert.Equal("multicolor", EnumText.ToText(Color.Multicolor));
    }

    [Fact]
    public void Parse_reads_back_every_value_that_ToText_writes()
    {
        AssertRoundTrip<Category>();
        AssertRoundTrip<Color>();
        AssertRoundTrip<ImportSource>();
        AssertRoundTrip<GarmentStatus>();
    }

    [Theory]
    [InlineData("Top")]
    [InlineData("0")]
    [InlineData("hat")]
    [InlineData("")]
    public void Parse_rejects_text_that_no_value_writes(string text)
    {
        Assert.Throws<InvalidOperationException>(() => EnumText.Parse<Category>(text));
    }

    [Fact]
    public void SqlList_quotes_every_value_in_declaration_order()
    {
        Assert.Equal("'manual', 'url', 'email'", EnumText.SqlList<ImportSource>());
    }

    private static void AssertRoundTrip<TEnum>()
        where TEnum : struct, Enum
    {
        foreach (var value in Enum.GetValues<TEnum>())
        {
            Assert.Equal(value, EnumText.Parse<TEnum>(EnumText.ToText(value)));
        }
    }
}
