using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Domain.Tests.Garments;

public class WardrobeFilterTests
{
    [Fact]
    public void Default_filter_targets_active_garments_with_no_criteria()
    {
        var filter = WardrobeFilter.Default;

        Assert.Equal(GarmentStatus.Active, filter.Status);
        Assert.Null(filter.Category);
        Assert.Null(filter.Color);
        Assert.Null(filter.Size);
        Assert.Null(filter.SearchText);
        Assert.False(filter.HasCriteria);
    }

    [Fact]
    public void Any_criterion_makes_has_criteria_true()
    {
        Assert.True((WardrobeFilter.Default with { Category = Category.Top }).HasCriteria);
        Assert.True((WardrobeFilter.Default with { Color = Color.Red }).HasCriteria);
        Assert.True((WardrobeFilter.Default with { Size = Size.Create("M") }).HasCriteria);
        Assert.True((WardrobeFilter.Default with { SearchText = "linen" }).HasCriteria);
    }

    [Fact]
    public void Status_alone_is_not_a_criterion()
    {
        var archived = WardrobeFilter.Default with { Status = GarmentStatus.Archived };

        Assert.False(archived.HasCriteria);
    }

    [Theory]
    [InlineData("  linen shirt ", "linen shirt")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void Search_text_is_trimmed_and_blank_becomes_null(string? input, string? expected)
    {
        var filter = new WardrobeFilter { SearchText = input };

        Assert.Equal(expected, filter.SearchText);
    }

    [Fact]
    public void Search_text_longer_than_the_maximum_is_rejected()
    {
        var text = new string('a', WardrobeFilter.MaxSearchTextLength + 1);

        var exception = Assert.Throws<DomainValidationException>(
            () => new WardrobeFilter { SearchText = text });

        Assert.Equal(WardrobeFilter.Errors.SearchTextTooLong, exception.Code);
    }

    [Fact]
    public void Filters_are_compared_by_value()
    {
        var a = new WardrobeFilter { Category = Category.Top, SearchText = " x " };
        var b = new WardrobeFilter { Category = Category.Top, SearchText = "x" };

        Assert.Equal(a, b);
    }
}
