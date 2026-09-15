using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Domain.Tests.Garments;

public class SizeTests
{
    [Theory]
    [InlineData("M", "M")]
    [InlineData(" 42 ", "42")]
    [InlineData("32x32", "32x32")]
    public void Create_keeps_trimmed_label(string input, string expected)
    {
        var size = Size.Create(input);

        Assert.Equal(expected, size.Label);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_label(string input)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Size.Create(input));

        Assert.Equal(Size.Errors.Empty, exception.Code);
    }

    [Fact]
    public void Create_rejects_label_longer_than_twenty_characters()
    {
        var label = new string('X', Size.MaxLength + 1);

        var exception = Assert.Throws<DomainValidationException>(() => Size.Create(label));

        Assert.Equal(Size.Errors.TooLong, exception.Code);
    }

    [Fact]
    public void Create_accepts_label_of_exactly_twenty_characters()
    {
        var label = new string('X', Size.MaxLength);

        Assert.Equal(label, Size.Create(label).Label);
    }

    [Fact]
    public void Create_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => Size.Create(null!));
    }

    [Fact]
    public void Equality_is_by_value_and_case_sensitive()
    {
        Assert.Equal(Size.Create("M"), Size.Create(" M "));
        Assert.NotEqual(Size.Create("M"), Size.Create("m"));
    }
}
