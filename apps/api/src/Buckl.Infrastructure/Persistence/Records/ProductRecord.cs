namespace Buckl.Infrastructure.Persistence.Records;

/// <summary>Row of <c>products</c>, one property per column. Converted to and from
/// <c>Product</c> by <c>ProductMapper</c>.</summary>
public sealed class ProductRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? ReferenceImageUrl { get; set; }

    public string? SourceUrl { get; set; }

    public string Source { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
