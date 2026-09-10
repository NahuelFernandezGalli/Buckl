using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Domain.Tests.Garments;

public class ClassificationTests
{
    [Fact]
    public void Create_keeps_category_color_and_size()
    {
        var size = Size.Create("M");

        var classification = Classification.Create(Category.Top, Color.Blue, size);

        Assert.Equal(Category.Top, classification.Category);
        Assert.Equal(Color.Blue, classification.Color);
        Assert.Equal(size, classification.Size);
    }

    [Fact]
    public void Size_is_optional()
    {
        var classification = Classification.Create(Category.Accessory, Color.Black);

        Assert.Null(classification.Size);
    }

    [Fact]
    public void Create_rejects_undefined_category()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Classification.Create((Category)99, Color.Blue));

        Assert.Equal(Classification.Errors.UnknownCategory, exception.Code);
    }

    [Fact]
    public void Create_rejects_undefined_color()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Classification.Create(Category.Top, (Color)99));

        Assert.Equal(Classification.Errors.UnknownColor, exception.Code);
    }

    [Fact]
    public void Category_has_the_six_values_from_the_design()
    {
        var expected = new[]
        {
            Category.Top, Category.Bottom, Category.Dress,
            Category.Outerwear, Category.Footwear, Category.Accessory,
        };

        Assert.Equal(expected, Enum.GetValues<Category>());
    }

    [Fact]
    public void Color_has_the_fourteen_values_from_the_design()
    {
        var expected = new[]
        {
            Color.Black, Color.White, Color.Grey, Color.Navy, Color.Blue, Color.Red, Color.Green,
            Color.Yellow, Color.Brown, Color.Beige, Color.Pink, Color.Purple, Color.Orange,
            Color.Multicolor,
        };

        Assert.Equal(expected, Enum.GetValues<Color>());
    }

    [Fact]
    public void Equality_is_by_value()
    {
        var a = Classification.Create(Category.Bottom, Color.Navy, Size.Create("32"));
        var b = Classification.Create(Category.Bottom, Color.Navy, Size.Create(" 32 "));

        Assert.Equal(a, b);
    }
}
