using Buckl.Infrastructure.Persistence.Repositories;

namespace Buckl.Infrastructure.Tests.Persistence;

public class LikePatternTests
{
    [Theory]
    [InlineData("shirt", "shirt")]
    [InlineData("100%", @"100\%")]
    [InlineData("a_b", @"a\_b")]
    [InlineData(@"back\slash", @"back\\slash")]
    public void Escape_makes_wildcards_literal(string text, string expected) =>
        Assert.Equal(expected, LikePattern.Escape(text));

    [Fact]
    public void Contains_wraps_the_escaped_text_in_wildcards() =>
        Assert.Equal(@"%100\%%", LikePattern.Contains("100%"));
}
