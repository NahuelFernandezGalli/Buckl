using Buckl.Domain.Products;

namespace Buckl.Application.Products;

/// <summary>One catalog product. Products are global and hold no personal data, so any
/// authenticated user may read any of them.</summary>
public sealed class GetProductHandler
{
    private readonly IProductRepository _products;

    public GetProductHandler(IProductRepository products)
    {
        _products = products;
    }

    public async Task<Product> HandleAsync(ProductId id, CancellationToken cancellationToken = default) =>
        await _products.GetByIdAsync(id, cancellationToken) ?? throw new ProductNotFoundException(id);
}
