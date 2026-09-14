using Buckl.Domain.Common;
using Buckl.Domain.Products;

namespace Buckl.Domain.Tests.Products;

public class ProductTests
{
    private static readonly Uri ImageUrl = new("https://cdn.example.com/shirt.jpg");
    private static readonly Uri PageUrl = new("https://store.example.com/products/shirt-123");

    [Fact]
    public void Create_sets_every_attribute_and_created_at_in_utc()
    {
        var now = new DateTimeOffset(2026, 9, 8, 9, 0, 0, TimeSpan.FromHours(-3));

        var product = Product.Create("Oxford shirt", ImportSource.Url, now, "Acme", ImageUrl, PageUrl);

        Assert.NotEqual(Guid.Empty, product.Id.Value);
        Assert.Equal("Oxford shirt", product.Name);
        Assert.Equal("Acme", product.Brand);
        Assert.Equal(ImageUrl, product.ReferenceImageUrl);
        Assert.Equal(PageUrl, product.SourceUrl);
        Assert.Equal(ImportSource.Url, product.Source);
        Assert.Equal(now.ToUniversalTime(), product.CreatedAt);
        Assert.Equal(TimeSpan.Zero, product.CreatedAt.Offset);
    }

    [Fact]
    public void Create_with_only_required_values_leaves_optionals_null()
    {
        var product = Product.Create("Plain tee", ImportSource.Manual, TestClock.Now);

        Assert.Null(product.Brand);
        Assert.Null(product.ReferenceImageUrl);
        Assert.Null(product.SourceUrl);
    }

    [Fact]
    public void Create_generates_a_distinct_id_each_time()
    {
        var first = Product.Create("Tee", ImportSource.Manual, TestClock.Now);
        var second = Product.Create("Tee", ImportSource.Manual, TestClock.Now);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Create_trims_name_and_brand()
    {
        var product = Product.Create("  Oxford shirt  ", ImportSource.Manual, TestClock.Now, "  Acme ");

        Assert.Equal("Oxford shirt", product.Name);
        Assert.Equal("Acme", product.Brand);
    }

    [Fact]
    public void Create_turns_blank_brand_into_null()
    {
        var product = Product.Create("Tee", ImportSource.Manual, TestClock.Now, "   ");

        Assert.Null(product.Brand);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_name(string name)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Create(name, ImportSource.Manual, TestClock.Now));

        Assert.Equal(Product.Errors.NameEmpty, exception.Code);
    }

    [Fact]
    public void Create_rejects_null_name()
    {
        Assert.Throws<ArgumentNullException>(
            () => Product.Create(null!, ImportSource.Manual, TestClock.Now));
    }

    [Fact]
    public void Create_rejects_name_longer_than_two_hundred_characters()
    {
        var name = new string('a', Product.MaxNameLength + 1);

        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Create(name, ImportSource.Manual, TestClock.Now));

        Assert.Equal(Product.Errors.NameTooLong, exception.Code);
    }

    [Fact]
    public void Create_rejects_brand_longer_than_one_hundred_characters()
    {
        var brand = new string('b', Product.MaxBrandLength + 1);

        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Create("Tee", ImportSource.Manual, TestClock.Now, brand));

        Assert.Equal(Product.Errors.BrandTooLong, exception.Code);
    }

    [Theory]
    [InlineData("http://cdn.example.com/shirt.jpg")]
    [InlineData("ftp://cdn.example.com/shirt.jpg")]
    public void Create_rejects_reference_image_url_that_is_not_https(string url)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Create("Tee", ImportSource.Url, TestClock.Now, referenceImageUrl: new Uri(url)));

        Assert.Equal(Product.Errors.ImageUrlNotHttps, exception.Code);
    }

    [Fact]
    public void Create_rejects_relative_reference_image_url()
    {
        var relative = new Uri("/images/shirt.jpg", UriKind.Relative);

        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Create("Tee", ImportSource.Url, TestClock.Now, referenceImageUrl: relative));

        Assert.Equal(Product.Errors.ImageUrlNotHttps, exception.Code);
    }

    [Theory]
    [InlineData("http://store.example.com/p/1")]
    [InlineData("https://store.example.com/p/1")]
    public void Create_accepts_http_and_https_source_urls(string url)
    {
        var product = Product.Create("Tee", ImportSource.Url, TestClock.Now, sourceUrl: new Uri(url));

        Assert.Equal(new Uri(url), product.SourceUrl);
    }

    [Fact]
    public void Create_rejects_source_url_with_another_scheme()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Create("Tee", ImportSource.Url, TestClock.Now, sourceUrl: new Uri("file:///C:/x.html")));

        Assert.Equal(Product.Errors.SourceUrlInvalid, exception.Code);
    }

    [Fact]
    public void Create_rejects_undefined_source()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Product.Create("Tee", (ImportSource)99, TestClock.Now));

        Assert.Equal(Product.Errors.UnknownSource, exception.Code);
    }
}
