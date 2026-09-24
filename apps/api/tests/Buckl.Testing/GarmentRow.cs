using Buckl.Domain.Garments;

namespace Buckl.Testing;

/// <summary>A garments row for raw inserts. The defaults are valid; a test overrides only the
/// column it is about, so a constraint violation always has one cause.</summary>
public sealed record GarmentRow
{
    public GarmentId Id { get; init; } = GarmentId.New();

    public Guid? ProductId { get; init; }

    public string? PhotoKey { get; init; }

    public string Category { get; init; } = "top";

    public string Color { get; init; } = "blue";

    public string? Size { get; init; } = "M";

    public decimal? PurchaseAmount { get; init; }

    public string? PurchaseCurrency { get; init; }

    public DateOnly? PurchaseDate { get; init; }

    public string Source { get; init; } = "manual";

    public string Status { get; init; } = "active";

    public string? Notes { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = TestClock.Now;

    public DateTimeOffset? ArchivedAt { get; init; }
}
