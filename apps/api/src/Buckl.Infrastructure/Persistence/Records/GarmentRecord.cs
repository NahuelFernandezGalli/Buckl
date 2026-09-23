namespace Buckl.Infrastructure.Persistence.Records;

/// <summary>Row of <c>garments</c>, one property per column. Converted to and from
/// <c>Garment</c> by <c>GarmentMapper</c>; <see cref="Product"/> exists only so queries can
/// search by the linked product's name and brand.</summary>
public sealed class GarmentRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? ProductId { get; set; }

    public ProductRecord? Product { get; set; }

    public string? PhotoKey { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string? Size { get; set; }

    public decimal? PurchaseAmount { get; set; }

    public string? PurchaseCurrency { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }
}
