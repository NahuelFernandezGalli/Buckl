namespace Buckl.Domain.Products;

/// <summary>Persistence port for catalog products, implemented in the infrastructure layer in
/// phase 4. Products are global, so no owner argument appears here.
/// <see cref="FindBySourceUrlAsync"/> supports reusing a product on import in phase 7.</summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    Task<Product?> FindBySourceUrlAsync(Uri sourceUrl, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
