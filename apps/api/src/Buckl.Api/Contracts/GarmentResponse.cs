using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Api.Contracts;

/// <summary>A garment as the web app's <c>Garment</c> type expects it
/// (apps/web/src/domain/garment.ts). <see cref="PhotoUrl"/> is always null until phase 6 signs
/// photo URLs.</summary>
public sealed record GarmentResponse(
    Guid Id,
    Guid? ProductId,
    string? PhotoUrl,
    ClassificationResponse Classification,
    PurchaseInfoResponse? PurchaseInfo,
    ImportSource Source,
    GarmentStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ArchivedAt)
{
    public static GarmentResponse From(Garment garment)
    {
        ArgumentNullException.ThrowIfNull(garment);

        return new GarmentResponse(
            garment.Id.Value,
            garment.ProductId?.Value,
            PhotoUrl: null,
            new ClassificationResponse(
                garment.Classification.Category,
                garment.Classification.Color,
                garment.Classification.Size?.Label),
            garment.PurchaseInfo is { } purchase
                ? new PurchaseInfoResponse(
                    new MoneyResponse(purchase.Price.Amount, purchase.Price.Currency),
                    purchase.Date)
                : null,
            garment.Source,
            garment.Status,
            garment.Notes,
            garment.CreatedAt,
            garment.UpdatedAt,
            garment.ArchivedAt);
    }
}

public sealed record ClassificationResponse(Category Category, Color Color, string? Size);

public sealed record PurchaseInfoResponse(MoneyResponse Price, DateOnly Date);

public sealed record MoneyResponse(decimal Amount, string Currency);
