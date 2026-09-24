using Buckl.Domain.Common;
using Buckl.Domain.Products;

namespace Buckl.Api.Contracts;

/// <summary>A catalog product as the web app's <c>Product</c> type expects it.</summary>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Brand,
    string? ReferenceImageUrl,
    string? SourceUrl,
    ImportSource Source,
    DateTimeOffset CreatedAt)
{
    public static ProductResponse From(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductResponse(
            product.Id.Value,
            product.Name,
            product.Brand,
            product.ReferenceImageUrl?.AbsoluteUri,
            product.SourceUrl?.AbsoluteUri,
            product.Source,
            product.CreatedAt);
    }
}
