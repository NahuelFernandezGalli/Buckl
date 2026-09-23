using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Infrastructure.Persistence.Repositories;
using Buckl.Testing;

namespace Buckl.Infrastructure.Tests.Persistence;

public class ProductRepositoryTests
{
    private readonly PostgresDatabase _database;

    public ProductRepositoryTests(PostgresDatabase database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_stored_product_is_read_back_by_id()
    {
        var product = NewProduct();
        await StoreAsync(product);

        var restored = await ReadAsync(repository => repository.GetByIdAsync(product.Id, Ct));

        Assert.NotNull(restored);
        Assert.Equal(product.Name, restored.Name);
        Assert.Equal(product.SourceUrl, restored.SourceUrl);
    }

    [Fact]
    public async Task GetById_returns_null_for_an_unknown_product()
    {
        var restored = await ReadAsync(repository => repository.GetByIdAsync(ProductId.New(), Ct));

        Assert.Null(restored);
    }

    [Fact]
    public async Task FindBySourceUrl_returns_the_product_imported_from_that_page()
    {
        var product = NewProduct();
        await StoreAsync(product);

        var found = await ReadAsync(repository => repository.FindBySourceUrlAsync(product.SourceUrl!, Ct));

        Assert.Equal(product.Id, found?.Id);
    }

    [Fact]
    public async Task FindBySourceUrl_returns_null_when_no_product_came_from_that_page()
    {
        var found = await ReadAsync(repository => repository.FindBySourceUrlAsync(
            new Uri($"https://example.com/{Guid.NewGuid():N}"),
            Ct));

        Assert.Null(found);
    }

    private static Product NewProduct() => Product.Create(
        "Oxford shirt",
        ImportSource.Url,
        TestClock.Now,
        brand: "Uniqlo",
        sourceUrl: new Uri($"https://example.com/products/{Guid.NewGuid():N}"));

    private async Task StoreAsync(Product product)
    {
        var owner = await _database.InsertUserAsync(Ct);
        await using var scope = await UserScope.BeginAsync(_database, owner, Ct);
        await new EfProductRepository(scope.Context).AddAsync(product, Ct);
        await scope.SaveAndCommitAsync(Ct);
    }

    private async Task<Product?> ReadAsync(Func<EfProductRepository, Task<Product?>> read)
    {
        var reader = await _database.InsertUserAsync(Ct);
        await using var scope = await UserScope.BeginAsync(_database, reader, Ct);

        return await read(new EfProductRepository(scope.Context));
    }
}
