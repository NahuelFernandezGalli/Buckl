using Buckl.Domain.Common;

namespace Buckl.Domain.Tests.Common;

public class DomainExceptionTests
{
    [Fact]
    public void Validation_exception_exposes_code_and_message()
    {
        var exception = new DomainValidationException("sample.code", "Something is invalid.");

        Assert.Equal("sample.code", exception.Code);
        Assert.Equal("Something is invalid.", exception.Message);
        Assert.IsAssignableFrom<DomainException>(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validation_exception_rejects_blank_code(string code)
    {
        Assert.Throws<ArgumentException>(() => new DomainValidationException(code, "message"));
    }
}
