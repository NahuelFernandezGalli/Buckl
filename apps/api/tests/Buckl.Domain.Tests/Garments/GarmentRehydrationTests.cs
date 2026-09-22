using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Garments;

public class GarmentRehydrationTests
{
    private static readonly DateTimeOffset Created = TestClock.Now;

    private static readonly DateTimeOffset Updated = TestClock.Now.AddDays(1);

    private static GarmentSnapshot ActiveSnapshot() => new(
        Id: GarmentId.New(),
        OwnerId: GarmentMother.DefaultOwner,
        Classification: GarmentMother.BlueTop(),
        Source: ImportSource.Url,
        Status: GarmentStatus.Active,
        PhotoKey: GarmentMother.PhotoFor(GarmentMother.DefaultOwner),
        PurchaseInfo: PurchaseInfo.Rehydrate(Money.Create(49.90m, "USD"), new DateOnly(2026, 9, 1)),
        ProductId: ProductId.New(),
        Notes: "Oxford shirt",
        CreatedAt: Created,
        UpdatedAt: Updated,
        ArchivedAt: null);

    [Fact]
    public void Rehydrate_restores_every_field_of_an_active_garment()
    {
        var snapshot = ActiveSnapshot();

        var garment = Garment.Rehydrate(snapshot);

        Assert.Equal(snapshot.Id, garment.Id);
        Assert.Equal(snapshot.OwnerId, garment.OwnerId);
        Assert.Equal(snapshot.Classification, garment.Classification);
        Assert.Equal(snapshot.Source, garment.Source);
        Assert.Equal(GarmentStatus.Active, garment.Status);
        Assert.Equal(snapshot.PhotoKey, garment.PhotoKey);
        Assert.Equal(snapshot.PurchaseInfo, garment.PurchaseInfo);
        Assert.Equal(snapshot.ProductId, garment.ProductId);
        Assert.Equal("Oxford shirt", garment.Notes);
        Assert.Equal(Created, garment.CreatedAt);
        Assert.Equal(Updated, garment.UpdatedAt);
        Assert.Null(garment.ArchivedAt);
    }

    [Fact]
    public void Rehydrate_restores_an_archived_garment()
    {
        var garment = Garment.Rehydrate(ActiveSnapshot() with
        {
            Status = GarmentStatus.Archived,
            ArchivedAt = Updated,
        });

        Assert.True(garment.IsArchived);
        Assert.Equal(Updated, garment.ArchivedAt);
    }

    [Fact]
    public void A_rehydrated_archived_garment_is_still_read_only()
    {
        var garment = Garment.Rehydrate(ActiveSnapshot() with
        {
            Status = GarmentStatus.Archived,
            ArchivedAt = Updated,
        });

        var exception = Assert.Throws<ArchivedGarmentIsReadOnlyException>(
            () => garment.UpdateNotes("changed", Updated.AddHours(1)));

        Assert.Equal(ArchivedGarmentIsReadOnlyException.ErrorCode, exception.Code);
    }

    [Fact]
    public void Rehydrate_converts_timestamps_to_utc()
    {
        var local = new DateTimeOffset(2026, 9, 8, 9, 0, 0, TimeSpan.FromHours(-3));

        var garment = Garment.Rehydrate(ActiveSnapshot() with { CreatedAt = local, UpdatedAt = local });

        Assert.Equal(TimeSpan.Zero, garment.CreatedAt.Offset);
        Assert.Equal(local, garment.CreatedAt);
        Assert.Equal(TimeSpan.Zero, garment.UpdatedAt.Offset);
    }

    [Fact]
    public void Rehydrate_rejects_an_archived_status_without_archive_time()
    {
        var snapshot = ActiveSnapshot() with { Status = GarmentStatus.Archived, ArchivedAt = null };

        var exception = Assert.Throws<DomainValidationException>(() => Garment.Rehydrate(snapshot));

        Assert.Equal(Garment.Errors.InconsistentArchiveState, exception.Code);
    }

    [Fact]
    public void Rehydrate_rejects_an_archive_time_on_an_active_garment()
    {
        var snapshot = ActiveSnapshot() with { ArchivedAt = Updated };

        var exception = Assert.Throws<DomainValidationException>(() => Garment.Rehydrate(snapshot));

        Assert.Equal(Garment.Errors.InconsistentArchiveState, exception.Code);
    }

    [Fact]
    public void Rehydrate_rejects_an_update_before_creation()
    {
        var snapshot = ActiveSnapshot() with { UpdatedAt = Created.AddSeconds(-1) };

        var exception = Assert.Throws<DomainValidationException>(() => Garment.Rehydrate(snapshot));

        Assert.Equal(Garment.Errors.UpdatedBeforeCreated, exception.Code);
    }

    [Fact]
    public void Rehydrate_rejects_a_photo_of_another_owner()
    {
        var snapshot = ActiveSnapshot() with { PhotoKey = GarmentMother.PhotoFor(UserId.New()) };

        var exception = Assert.Throws<DomainValidationException>(() => Garment.Rehydrate(snapshot));

        Assert.Equal(Garment.Errors.PhotoNotOwned, exception.Code);
    }

    [Fact]
    public void Rehydrate_rejects_an_unknown_status()
    {
        var snapshot = ActiveSnapshot() with { Status = (GarmentStatus)99 };

        var exception = Assert.Throws<DomainValidationException>(() => Garment.Rehydrate(snapshot));

        Assert.Equal(Garment.Errors.UnknownStatus, exception.Code);
    }

    [Fact]
    public void Rehydrate_rejects_an_unknown_source()
    {
        var snapshot = ActiveSnapshot() with { Source = (ImportSource)99 };

        var exception = Assert.Throws<DomainValidationException>(() => Garment.Rehydrate(snapshot));

        Assert.Equal(Garment.Errors.UnknownSource, exception.Code);
    }

    [Fact]
    public void Rehydrate_rejects_notes_over_the_limit()
    {
        var snapshot = ActiveSnapshot() with { Notes = new string('x', Garment.MaxNotesLength + 1) };

        var exception = Assert.Throws<DomainValidationException>(() => Garment.Rehydrate(snapshot));

        Assert.Equal(Garment.Errors.NotesTooLong, exception.Code);
    }
}
