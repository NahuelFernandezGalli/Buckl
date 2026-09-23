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
        GarmentStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? archivedAt)
    {
        Id = id;
        OwnerId = ownerId;
        Classification = classification;
        Source = source;
        PhotoKey = photoKey;
        PurchaseInfo = purchaseInfo;
        ProductId = productId;
        Notes = notes;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ArchivedAt = archivedAt;
    }

    public GarmentId Id { get; }

    /// <summary>The owner. Maps to <c>garments.user_id</c>, the column Row-Level Security
    /// compares against the session variable.</summary>
    public UserId OwnerId { get; }

    public ProductId? ProductId { get; }

    public PhotoKey? PhotoKey { get; private set; }

    public Classification Classification { get; private set; }

    public PurchaseInfo? PurchaseInfo { get; private set; }

    public ImportSource Source { get; }

    public GarmentStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

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

        EnsureKnown(source);

        EnsureOwnedBy(photoKey, ownerId);

        var createdAt = now.ToUniversalTime();

        return new Garment(
            GarmentId.New(),
            ownerId,
            classification,
            source,
            photoKey,
            purchaseInfo,
            productId,
            NormalizeNotes(notes),
            GarmentStatus.Active,
            createdAt,
            createdAt,
            archivedAt: null);
    }

    /// <summary>Rebuilds a garment that was created earlier and stored. Only persistence adapters
    /// call it. It re-checks the invariants that tie fields together (owner of the photo, archive
    /// time matching the status, notes length, update not before creation) but applies no creation
    /// or transition rule, because a stored garment may already be archived.</summary>
    public static Garment Rehydrate(GarmentSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(snapshot.Classification);

        EnsureKnown(snapshot.Source);

        if (!Enum.IsDefined(snapshot.Status))
        {
            throw new DomainValidationException(
                Errors.UnknownStatus,
                $"Unknown garment status '{snapshot.Status}'.");
        }

        EnsureOwnedBy(snapshot.PhotoKey, snapshot.OwnerId);

        if ((snapshot.Status == GarmentStatus.Archived) != snapshot.ArchivedAt.HasValue)
        {
            throw new DomainValidationException(
                Errors.InconsistentArchiveState,
                "An archived garment needs an archive time, and an active one cannot have one.");
        }

        if (snapshot.UpdatedAt < snapshot.CreatedAt)
        {
            throw new DomainValidationException(
                Errors.UpdatedBeforeCreated,
                "A garment cannot be updated before it was created.");
        }

        return new Garment(
            snapshot.Id,
            snapshot.OwnerId,
            snapshot.Classification,
            snapshot.Source,
            snapshot.PhotoKey,
            snapshot.PurchaseInfo,
            snapshot.ProductId,
            NormalizeNotes(snapshot.Notes),
            snapshot.Status,
            snapshot.CreatedAt.ToUniversalTime(),
            snapshot.UpdatedAt.ToUniversalTime(),
            snapshot.ArchivedAt?.ToUniversalTime());
    }

    /// <summary>Replaces category, color and size label.</summary>
    /// <param name="classification">The new classification.</param>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    public void UpdateClassification(Classification classification, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(classification);
        EnsureEditable();

        Classification = classification;
        Touch(now);
    }

    /// <summary>Sets or clears what was paid and when.</summary>
    /// <param name="purchaseInfo">The new purchase information, or null to clear it.</param>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    public void UpdatePurchaseInfo(PurchaseInfo? purchaseInfo, DateTimeOffset now)
    {
        EnsureEditable();

        PurchaseInfo = purchaseInfo;
        Touch(now);
    }

    /// <summary>Points the garment at a different photo, which must belong to its owner.</summary>
    /// <param name="photoKey">The new photo key.</param>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    public void ReplacePhoto(PhotoKey photoKey, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(photoKey);
        EnsureEditable();
        EnsureOwnedBy(photoKey, OwnerId);

        PhotoKey = photoKey;
        Touch(now);
    }

    /// <summary>Sets or clears the free-text note.</summary>
    /// <param name="notes">The new note, or null or blank to clear it.</param>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    public void UpdateNotes(string? notes, DateTimeOffset now)
    {
        EnsureEditable();

        Notes = NormalizeNotes(notes);
        Touch(now);
    }

    /// <summary>Hides the garment from the wardrobe without deleting it.</summary>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    public void Archive(DateTimeOffset now)
    {
        if (IsArchived)
        {
            throw new GarmentAlreadyArchivedException(Id);
        }

        Status = GarmentStatus.Archived;
        ArchivedAt = now.ToUniversalTime();
        Touch(now);
    }

    /// <summary>Puts an archived garment back into the wardrobe.</summary>
    /// <param name="now">The caller's current instant; stored in UTC.</param>
    public void Restore(DateTimeOffset now)
    {
        if (!IsArchived)
        {
            throw new GarmentNotArchivedException(Id);
        }

        Status = GarmentStatus.Active;
        ArchivedAt = null;
        Touch(now);
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now.ToUniversalTime();

    /// <summary>An archived garment is a record, not part of the wardrobe: it must be restored
    /// before it can change.</summary>
    private void EnsureEditable()
    {
        if (IsArchived)
        {
            throw new ArchivedGarmentIsReadOnlyException(Id);
        }
    }

    private static void EnsureKnown(ImportSource source)
    {
        if (!Enum.IsDefined(source))
        {
            throw new DomainValidationException(
                Errors.UnknownSource,
                $"Unknown import source '{source}'.");
        }
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
        public const string UnknownStatus = "garment.unknown_status";
        public const string InconsistentArchiveState = "garment.inconsistent_archive_state";
        public const string UpdatedBeforeCreated = "garment.updated_before_created";
    }
}
