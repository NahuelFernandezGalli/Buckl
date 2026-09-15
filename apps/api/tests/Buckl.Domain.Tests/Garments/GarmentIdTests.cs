using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Domain.Tests.Garments;

public class GarmentIdTests
{
    [Fact]
    public void New_generates_distinct_non_empty_ids()
    {
        Assert.NotEqual(GarmentId.New(), GarmentId.New());
        Assert.NotEqual(Guid.Empty, GarmentId.New().Value);
    }

    [Fact]
    public void Constructor_rejects_empty_guid()
    {
        var exception = Assert.Throws<DomainValidationException>(() => new GarmentId(Guid.Empty));

        Assert.Equal(GarmentId.Errors.Empty, exception.Code);
    }
}
