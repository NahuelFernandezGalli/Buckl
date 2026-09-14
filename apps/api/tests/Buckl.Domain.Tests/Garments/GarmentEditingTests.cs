using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Garments;

public class GarmentEditingTests
{
    private static readonly DateTimeOffset Later = TestClock.Now.AddHours(1);

    [Fact]
    public void UpdateClassification_replaces_it_and_touches_updated_at()
    {
        var garment = GarmentMother.Active();
        var jeans = Classification.Create(Category.Bottom, Color.Navy, Size.Create("32"));

        garment.UpdateClassification(jeans, Later);

        Assert.Equal(jeans, garment.Classification);
        Assert.Equal(Later, garment.UpdatedAt);
        Assert.Equal(TestClock.Now, garment.CreatedAt);
    }

    [Fact]
    public void UpdateClassification_rejects_null()
    {
        var garment = GarmentMother.Active();

        Assert.Throws<ArgumentNullException>(() => garment.UpdateClassification(null!, Later));
    }

    [Fact]
    public void UpdatePurchaseInfo_sets_and_clears_it()
    {
        var garment = GarmentMother.Active();
        var purchase = PurchaseInfo.Create(Money.Create(80m, "USD"), TestClock.Today, TestClock.Today);

        garment.UpdatePurchaseInfo(purchase, Later);
        Assert.Equal(purchase, garment.PurchaseInfo);

        garment.UpdatePurchaseInfo(null, Later.AddMinutes(1));
        Assert.Null(garment.PurchaseInfo);
        Assert.Equal(Later.AddMinutes(1), garment.UpdatedAt);
    }

    [Fact]
    public void ReplacePhoto_accepts_a_key_owned_by_the_same_user()
    {
        var garment = GarmentMother.Active();
        var photo = GarmentMother.PhotoFor(garment.OwnerId, "new.jpg");

        garment.ReplacePhoto(photo, Later);

        Assert.Equal(photo, garment.PhotoKey);
        Assert.Equal(Later, garment.UpdatedAt);
    }

    [Fact]
    public void ReplacePhoto_rejects_a_key_owned_by_another_user()
    {
        var garment = GarmentMother.Active();
        var foreign = GarmentMother.PhotoFor(UserId.New());

        var exception = Assert.Throws<DomainValidationException>(
            () => garment.ReplacePhoto(foreign, Later));

        Assert.Equal(Garment.Errors.PhotoNotOwned, exception.Code);
        Assert.Null(garment.PhotoKey);
    }

    [Fact]
    public void ReplacePhoto_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => GarmentMother.Active().ReplacePhoto(null!, Later));
    }

    [Fact]
    public void UpdateNotes_trims_and_turns_blank_into_null()
    {
        var garment = GarmentMother.Active();

        garment.UpdateNotes("  Gift from mom ", Later);
        Assert.Equal("Gift from mom", garment.Notes);

        garment.UpdateNotes("   ", Later);
        Assert.Null(garment.Notes);
    }

    [Fact]
    public void UpdateNotes_rejects_notes_longer_than_five_hundred_characters()
    {
        var garment = GarmentMother.Active();

        var exception = Assert.Throws<DomainValidationException>(
            () => garment.UpdateNotes(new string('n', Garment.MaxNotesLength + 1), Later));

        Assert.Equal(Garment.Errors.NotesTooLong, exception.Code);
    }

    [Fact]
    public void Updating_the_classification_of_an_archived_garment_throws_read_only()
    {
        var garment = ArchivedGarment();
        var updatedAtBefore = garment.UpdatedAt;

        var exception = Assert.Throws<ArchivedGarmentIsReadOnlyException>(
            () => garment.UpdateClassification(GarmentMother.BlueTop(Size.Create("L")), Later));

        Assert.Equal(ArchivedGarmentIsReadOnlyException.ErrorCode, exception.Code);
        Assert.Equal(garment.Id, exception.GarmentId);
        Assert.Equal(updatedAtBefore, garment.UpdatedAt);
    }

    [Fact]
    public void Updating_the_purchase_info_of_an_archived_garment_throws_read_only()
    {
        var garment = ArchivedGarment();

        Assert.Throws<ArchivedGarmentIsReadOnlyException>(
            () => garment.UpdatePurchaseInfo(null, Later));
    }

    [Fact]
    public void Replacing_the_photo_of_an_archived_garment_throws_read_only()
    {
        var garment = ArchivedGarment();

        Assert.Throws<ArchivedGarmentIsReadOnlyException>(
            () => garment.ReplacePhoto(GarmentMother.PhotoFor(garment.OwnerId, "x.jpg"), Later));
    }

    [Fact]
    public void Updating_the_notes_of_an_archived_garment_throws_read_only()
    {
        var garment = ArchivedGarment();

        Assert.Throws<ArchivedGarmentIsReadOnlyException>(() => garment.UpdateNotes("note", Later));
    }

    [Fact]
    public void Editing_works_again_after_restore()
    {
        var garment = ArchivedGarment();
        garment.Restore(Later);

        garment.UpdateNotes("back", Later.AddMinutes(1));

        Assert.Equal("back", garment.Notes);
    }

    private static Garment ArchivedGarment()
    {
        var garment = GarmentMother.Active();
        garment.Archive(TestClock.Now);
        return garment;
    }
}
