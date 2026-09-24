using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence.Records;

namespace Buckl.Infrastructure.Persistence.Mapping;

/// <summary>Converts between the <see cref="Garment"/> aggregate and its row (ADR-0023). Reading
/// goes through <see cref="Garment.Rehydrate"/>, so a row that breaks an invariant fails loudly
/// instead of producing an invalid aggregate.</summary>
public static class GarmentMapper
{
    public static GarmentRecord ToRecord(Garment garment)
    {
        ArgumentNullException.ThrowIfNull(garment);

        var record = new GarmentRecord
        {
            Id = garment.Id.Value,
            UserId = garment.OwnerId.Value,
            CreatedAt = garment.CreatedAt,
        };
        Apply(garment, record);

        return record;
    }

    /// <summary>Copies every field that can change after creation onto a record, usually one the
    /// context is tracking, so EF Core updates only what changed.</summary>
    public static void Apply(Garment garment, GarmentRecord record)
    {
        ArgumentNullException.ThrowIfNull(garment);
        ArgumentNullException.ThrowIfNull(record);

        record.ProductId = garment.ProductId?.Value;
        record.PhotoKey = garment.PhotoKey?.Value;
        record.Category = EnumText.ToText(garment.Classification.Category);
        record.Color = EnumText.ToText(garment.Classification.Color);
        record.Size = garment.Classification.Size?.Label;
        record.PurchaseAmount = garment.PurchaseInfo?.Price.Amount;
        record.PurchaseCurrency = garment.PurchaseInfo?.Price.Currency;
        record.PurchaseDate = garment.PurchaseInfo?.Date;
        record.Source = EnumText.ToText(garment.Source);
        record.Status = EnumText.ToText(garment.Status);
        record.Notes = garment.Notes;
        record.UpdatedAt = garment.UpdatedAt;
        record.ArchivedAt = garment.ArchivedAt;
    }

    public static Garment ToDomain(GarmentRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var classification = Classification.Create(
            EnumText.Parse<Category>(record.Category),
            EnumText.Parse<Color>(record.Color),
            record.Size is null ? null : Size.Create(record.Size));

        return Garment.Rehydrate(new GarmentSnapshot(
            new GarmentId(record.Id),
            new UserId(record.UserId),
            classification,
            EnumText.Parse<ImportSource>(record.Source),
            EnumText.Parse<GarmentStatus>(record.Status),
            record.PhotoKey is null ? null : PhotoKey.Parse(record.PhotoKey),
            ToPurchaseInfo(record),
            record.ProductId is { } productId ? new ProductId(productId) : null,
            record.Notes,
            record.CreatedAt,
            record.UpdatedAt,
            record.ArchivedAt));
    }

    private static PurchaseInfo? ToPurchaseInfo(GarmentRecord record) =>
        record is { PurchaseAmount: { } amount, PurchaseCurrency: { } currency, PurchaseDate: { } date }
            ? PurchaseInfo.Rehydrate(Money.Create(amount, currency), date)
            : null;
}
