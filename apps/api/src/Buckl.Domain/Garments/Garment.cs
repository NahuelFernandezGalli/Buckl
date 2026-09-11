using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Domain.Users;

namespace Buckl.Domain.Garments;

/// <summary>A physical piece of clothing owned by one user, and the central aggregate of Buckl.
/// A garment belongs to exactly one owner, and the owner never changes.</summary>
public sealed class Garment
{
    public const int MaxNotesLength = 500;

    private Garment(
        GarmentId id,
        UserId ownerId,
        Classification classification,
        ImportSource source,
        PhotoKey? photoKey,
        PurchaseInfo? purchaseInfo,
        ProductId? productId,
        string? notes,
        DateTimeOffset createdAt)
    {
        Id = id;
        OwnerId = ownerId;
        Classification = classification;
        Source = source;
        PhotoKey = photoKey;
        PurchaseInfo = purchaseInfo;
        ProductId = productId;
        Notes = notes;
        Status = GarmentStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public GarmentId Id { get; }

    /// <summary>The owner. Maps to <c>garments.user_id</c>, the column Row-Level Security
    /// compares against the session variable.</summary>
    public UserId OwnerId { get; }

    public ProductId? ProductId { get; }

    public PhotoKey? PhotoKey { get; }

    public Classification Classification { get; }

    public PurchaseInfo? PurchaseInfo { get; }

    public ImportSource Source { get; }

    public GarmentStatus Status { get; }

    public string? Notes { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; }

    public DateTimeOffset? ArchivedAt { get; }

    public bool IsArchived => Status == GarmentStatus.Archived;

    /// <param name="ownerId">Who owns the garment; never changes afterwards.</param>
    /// <param name="classification">Category, color and optional size.</param>
    /// <param name="source">How the garment entered the wardrobe.</param>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    /// <param name="photoKey">Optional photo, which must belong to the same owner.</param>
    /// <param name="purchaseInfo">Optional price and purchase date.</param>
    /// <param name="productId">Optional link to the catalog article.</param>
    /// <param name="notes">Optional free text, at most 500 characters.</param>
    public static Garment Create(
        UserId ownerId,
        Classification classification,
        ImportSource source,
        DateTimeOffset now,
        PhotoKey? photoKey = null,
        PurchaseInfo? purchaseInfo = null,
        ProductId? productId = null,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(classification);

        if (!Enum.IsDefined(source))
        {
            throw new DomainValidationException(
                Errors.UnknownSource,
                $"Unknown import source '{source}'.");
        }

        EnsureOwnedBy(photoKey, ownerId);

        return new Garment(
            GarmentId.New(),
            ownerId,
            classification,
            source,
            photoKey,
            purchaseInfo,
            productId,
            NormalizeNotes(notes),
            now.ToUniversalTime());
    }

    private static void EnsureOwnedBy(PhotoKey? photoKey, UserId ownerId)
    {
        if (photoKey is not null && photoKey.OwnerId != ownerId)
        {
            throw new DomainValidationException(
                Errors.PhotoNotOwned,
                "Photo key belongs to another user.");
        }
    }

    private static string? NormalizeNotes(string? notes)
    {
        var trimmed = notes?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > MaxNotesLength)
        {
            throw new DomainValidationException(
                Errors.NotesTooLong,
                $"Notes cannot exceed {MaxNotesLength} characters.");
        }

        return trimmed;
    }

    public static class Errors
    {
        public const string UnknownSource = "garment.unknown_source";
        public const string PhotoNotOwned = "garment.photo_not_owned";
        public const string NotesTooLong = "garment.notes_too_long";
    }
}
