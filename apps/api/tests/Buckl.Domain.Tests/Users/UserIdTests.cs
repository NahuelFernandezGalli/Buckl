using Buckl.Domain.Common;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Users;

public class UserIdTests
{
    [Fact]
    public void New_generates_distinct_non_empty_ids()
    {
        Assert.NotEqual(UserId.New(), UserId.New());
        Assert.NotEqual(Guid.Empty, UserId.New().Value);
    }

    [Fact]
    public void Constructor_rejects_empty_guid()
    {
        var exception = Assert.Throws<DomainValidationException>(() => new UserId(Guid.Empty));

        Assert.Equal(UserId.Errors.Empty, exception.Code);
    }
}
