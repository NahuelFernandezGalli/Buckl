using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Infrastructure.Persistence.Records;

namespace Buckl.Infrastructure.Persistence.Mapping;

/// <summary>Converts between <see cref="Product"/> and its row. URLs are stored in their absolute,
/// normalized form, which is also what <c>FindBySourceUrlAsync</c> compares.</summary>
public static class ProductMapper
{
    public static ProductRecord ToRecord(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductRecord
        {
            Id = product.Id.Value,
            Name = product.Name,
            Brand = product.Brand,
            ReferenceImageUrl = product.ReferenceImageUrl?.AbsoluteUri,
            SourceUrl = product.SourceUrl?.AbsoluteUri,
            Source = EnumText.ToText(product.Source),
            CreatedAt = product.CreatedAt,
        };
    }

    public static Product ToDomain(ProductRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return Product.Rehydrate(new ProductSnapshot(
            new ProductId(record.Id),
            record.Name,
            record.Brand,
            record.ReferenceImageUrl is null ? null : new Uri(record.ReferenceImageUrl),
            record.SourceUrl is null ? null : new Uri(record.SourceUrl),
            EnumText.Parse<ImportSource>(record.Source),
            record.CreatedAt));
    }
}
