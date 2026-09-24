using Buckl.Application.Products;
using Buckl.Application.Tests.Fakes;
using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Testing;

namespace Buckl.Application.Tests.Products;

public class GetProductHandlerTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HandleAsync_returns_the_product()
    {
        var product = Product.Create("Oxford shirt", ImportSource.Manual, TestClock.Now);
        var handler = new GetProductHandler(new InMemoryProductRepository(product));

        var found = await handler.HandleAsync(product.Id, Ct);

        Assert.Same(product, found);
    }

    [Fact]
    public async Task HandleAsync_reports_an_unknown_product_as_not_found()
    {
        var handler = new GetProductHandler(new InMemoryProductRepository());
        var id = ProductId.New();

        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() => handler.HandleAsync(id, Ct));

        Assert.Equal(ProductNotFoundException.ErrorCode, exception.Code);
        Assert.Equal(id, exception.ProductId);
    }
}
