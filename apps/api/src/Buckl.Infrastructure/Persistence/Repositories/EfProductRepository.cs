using Buckl.Domain.Products;
using Buckl.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Persistence.Repositories;

public sealed class EfProductRepository : IProductRepository
{
    private readonly BucklDbContext _context;

    public EfProductRepository(BucklDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var record = await _context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == id.Value, cancellationToken);

        return record is null ? null : ProductMapper.ToDomain(record);
    }

    public async Task<Product?> FindBySourceUrlAsync(
        Uri sourceUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceUrl);

        var url = sourceUrl.AbsoluteUri;
        var record = await _context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.SourceUrl == url, cancellationToken);

        return record is null ? null : ProductMapper.ToDomain(record);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(ProductMapper.ToRecord(product));

        return Task.CompletedTask;
    }
}
