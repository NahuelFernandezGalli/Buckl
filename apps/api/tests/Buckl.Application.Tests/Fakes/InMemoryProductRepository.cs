using Buckl.Domain.Products;

namespace Buckl.Application.Tests.Fakes;

internal sealed class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products;

    public InMemoryProductRepository(params Product[] products)
    {
        _products = [.. products];
    }

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_products.SingleOrDefault(product => product.Id == id));

    public Task<Product?> FindBySourceUrlAsync(Uri sourceUrl, CancellationToken cancellationToken = default) =>
        Task.FromResult(_products.SingleOrDefault(product => product.SourceUrl == sourceUrl));

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products.Add(product);

        return Task.CompletedTask;
    }
}
