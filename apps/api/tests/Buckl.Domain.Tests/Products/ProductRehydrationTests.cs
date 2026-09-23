using Buckl.Domain.Common;
using Buckl.Domain.Products;

namespace Buckl.Domain.Tests.Products;

public class ProductRehydrationTests
{
    private static ProductSnapshot Snapshot() => new(
        Id: ProductId.New(),
        Name: "Oxford shirt",
        Brand: "Uniqlo",
        ReferenceImageUrl: new Uri("https://example.com/shirt.jpg"),
        SourceUrl: new Uri("https://example.com/shirt"),
        Source: ImportSource.Url,
        CreatedAt: TestClock.Now);

    [Fact]
    public void Rehydrate_restores_every_field()
    {
        var snapshot = Snapshot();

        var product = Product.Rehydrate(snapshot);

        Assert.Equal(snapshot.Id, product.Id);
        Assert.Equal("Oxford shirt", product.Name);
        Assert.Equal("Uniqlo", product.Brand);
        Assert.Equal(snapshot.ReferenceImageUrl, product.ReferenceImageUrl);
        Assert.Equal(snapshot.SourceUrl, product.SourceUrl);
        Assert.Equal(ImportSource.Url, product.Source);
        Assert.Equal(TestClock.Now, product.CreatedAt);
    }

    [Fact]
    public void Rehydrate_applies_the_same_validation_as_creation()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Rehydrate(Snapshot() with { Name = "  " }));

        Assert.Equal(Product.Errors.NameEmpty, exception.Code);
    }

    [Fact]
    public void Rehydrate_rejects_an_unknown_source()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Rehydrate(Snapshot() with { Source = (ImportSource)99 }));

        Assert.Equal(Product.Errors.UnknownSource, exception.Code);
    }
}
