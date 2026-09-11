using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Garments;

public class GarmentCreationTests
{
    private static readonly UserId Owner = GarmentMother.DefaultOwner;

    [Fact]
    public void Create_with_required_values_yields_an_active_garment_with_timestamps_in_utc()
    {
        var now = new DateTimeOffset(2026, 9, 8, 9, 0, 0, TimeSpan.FromHours(-3));
        var classification = GarmentMother.BlueTop();

        var garment = Garment.Create(Owner, classification, ImportSource.Manual, now);

        Assert.NotEqual(Guid.Empty, garment.Id.Value);
        Assert.Equal(Owner, garment.OwnerId);
        Assert.Equal(classification, garment.Classification);
        Assert.Equal(ImportSource.Manual, garment.Source);
        Assert.Equal(GarmentStatus.Active, garment.Status);
        Assert.False(garment.IsArchived);
        Assert.Equal(now.ToUniversalTime(), garment.CreatedAt);
        Assert.Equal(garment.CreatedAt, garment.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, garment.CreatedAt.Offset);
        Assert.Null(garment.ArchivedAt);
        Assert.Null(garment.PhotoKey);
        Assert.Null(garment.PurchaseInfo);
        Assert.Null(garment.ProductId);
        Assert.Null(garment.Notes);
    }

    [Fact]
    public void Create_keeps_the_optional_values()
    {
        var photo = GarmentMother.PhotoFor(Owner);
        var purchase = PurchaseInfo.Create(Money.Create(30m, "ARS"), TestClock.Today, TestClock.Today);
        var productId = ProductId.New();

        var garment = Garment.Create(
            Owner,
            GarmentMother.BlueTop(),
            ImportSource.Url,
            TestClock.Now,
            photo,
            purchase,
            productId,
            "  Bought on sale  ");

        Assert.Equal(photo, garment.PhotoKey);
        Assert.Equal(purchase, garment.PurchaseInfo);
        Assert.Equal(productId, garment.ProductId);
        Assert.Equal("Bought on sale", garment.Notes);
    }

    [Fact]
    public void Create_generates_a_distinct_id_each_time()
    {
        Assert.NotEqual(GarmentMother.Active().Id, GarmentMother.Active().Id);
    }

    [Fact]
    public void Create_rejects_a_null_classification()
    {
        Assert.Throws<ArgumentNullException>(
            () => Garment.Create(Owner, null!, ImportSource.Manual, TestClock.Now));
    }

    [Fact]
    public void Create_rejects_an_undefined_source()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Garment.Create(Owner, GarmentMother.BlueTop(), (ImportSource)99, TestClock.Now));

        Assert.Equal(Garment.Errors.UnknownSource, exception.Code);
    }

    [Fact]
    public void Create_rejects_a_photo_owned_by_another_user()
    {
        var otherUsersPhoto = GarmentMother.PhotoFor(UserId.New());

        var exception = Assert.Throws<DomainValidationException>(
            () => Garment.Create(
                Owner,
                GarmentMother.BlueTop(),
                ImportSource.Manual,
                TestClock.Now,
                otherUsersPhoto));

        Assert.Equal(Garment.Errors.PhotoNotOwned, exception.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_turns_blank_notes_into_null(string notes)
    {
        var garment = Garment.Create(
            Owner, GarmentMother.BlueTop(), ImportSource.Manual, TestClock.Now, notes: notes);

        Assert.Null(garment.Notes);
    }

    [Fact]
    public void Create_rejects_notes_longer_than_five_hundred_characters()
    {
        var notes = new string('n', Garment.MaxNotesLength + 1);

        var exception = Assert.Throws<DomainValidationException>(
            () => Garment.Create(
                Owner, GarmentMother.BlueTop(), ImportSource.Manual, TestClock.Now, notes: notes));

        Assert.Equal(Garment.Errors.NotesTooLong, exception.Code);
    }

    [Fact]
    public void Create_accepts_notes_of_exactly_five_hundred_characters()
    {
        var notes = new string('n', Garment.MaxNotesLength);

        var garment = Garment.Create(
            Owner, GarmentMother.BlueTop(), ImportSource.Manual, TestClock.Now, notes: notes);

        Assert.Equal(notes, garment.Notes);
    }
}
