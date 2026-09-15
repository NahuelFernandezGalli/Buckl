using Buckl.Domain.Common;
using Buckl.Domain.Products;

namespace Buckl.Domain.Tests.Products;

public class ProductIdTests
{
    [Fact]
    public void New_generates_distinct_non_empty_ids()
    {
        var first = ProductId.New();
        var second = ProductId.New();

        Assert.NotEqual(Guid.Empty, first.Value);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Constructor_rejects_empty_guid()
    {
        var exception = Assert.Throws<DomainValidationException>(() => new ProductId(Guid.Empty));

        Assert.Equal(ProductId.Errors.Empty, exception.Code);
    }

    [Fact]
    public void Equality_is_by_value()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(new ProductId(guid), new ProductId(guid));
    }

    [Fact]
    public void ToString_returns_the_guid()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(guid.ToString(), new ProductId(guid).ToString());
    }
}
