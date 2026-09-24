using Buckl.Domain.Common;

namespace Buckl.Domain.Products;

/// <summary>Every field of a stored product, handed to <see cref="Product.Rehydrate"/> by a
/// persistence adapter.</summary>
public sealed record ProductSnapshot(
    ProductId Id,
    string Name,
    string? Brand,
    Uri? ReferenceImageUrl,
    Uri? SourceUrl,
    ImportSource Source,
    DateTimeOffset CreatedAt);
