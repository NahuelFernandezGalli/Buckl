using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Infrastructure.Persistence;
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
    public void ToDomain_reports_a_row_that_breaks_a_domain_rule_as_corrupt_data()
    {
        var record = ProductMapper.ToRecord(Product.Create("Oxford shirt", ImportSource.Url, TestClock.Now));
        record.Name = " ";

        var exception = Assert.Throws<CorruptRecordException>(() => ProductMapper.ToDomain(record));

        var rule = Assert.IsType<DomainValidationException>(exception.InnerException);
        Assert.Equal(Product.Errors.NameEmpty, rule.Code);
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
