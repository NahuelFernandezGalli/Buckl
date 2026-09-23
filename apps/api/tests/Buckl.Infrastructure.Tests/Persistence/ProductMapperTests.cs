using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Infrastructure.Persistence.Mapping;
using Buckl.Testing;

namespace Buckl.Infrastructure.Tests.Persistence;

public class ProductMapperTests
{
    [Fact]
    public void A_product_survives_the_round_trip()
    {
        var product = Product.Create(
            "Oxford shirt",
            ImportSource.Url,
            TestClock.Now,
            brand: "Uniqlo",
            referenceImageUrl: new Uri("https://example.com/shirt.jpg"),
            sourceUrl: new Uri("https://example.com/shirt"));

        var restored = ProductMapper.ToDomain(ProductMapper.ToRecord(product));

        Assert.Equal(product.Id, restored.Id);
        Assert.Equal(product.Name, restored.Name);
        Assert.Equal(product.Brand, restored.Brand);
        Assert.Equal(product.ReferenceImageUrl, restored.ReferenceImageUrl);
        Assert.Equal(product.SourceUrl, restored.SourceUrl);
        Assert.Equal(product.Source, restored.Source);
        Assert.Equal(product.CreatedAt, restored.CreatedAt);
    }

    [Fact]
    public void ToRecord_stores_urls_in_their_absolute_normalized_form()
    {
        var product = Product.Create(
            "Shirt",
            ImportSource.Url,
            TestClock.Now,
            sourceUrl: new Uri("HTTPS://Example.com/a shirt"));

        var record = ProductMapper.ToRecord(product);

        Assert.Equal("https://example.com/a%20shirt", record.SourceUrl);
    }
}
