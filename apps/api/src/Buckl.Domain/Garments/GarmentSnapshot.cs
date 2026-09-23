using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Domain.Users;

namespace Buckl.Domain.Garments;

/// <summary>Every field of a stored garment, as read by a persistence adapter and handed to
/// <see cref="Garment.Rehydrate"/>. A parameter object, not a second model of the
/// garment.</summary>
public sealed record GarmentSnapshot(
    GarmentId Id,
    UserId OwnerId,
    Classification Classification,
    ImportSource Source,
    GarmentStatus Status,
    PhotoKey? PhotoKey,
    PurchaseInfo? PurchaseInfo,
    ProductId? ProductId,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ArchivedAt);
