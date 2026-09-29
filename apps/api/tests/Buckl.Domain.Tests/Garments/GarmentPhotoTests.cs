using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Garments;

public class GarmentPhotoTests
{
    private static readonly UserId Owner = UserId.New();

    private static PhotoKey PhotoOf(UserId owner) =>
        PhotoKey.ForGarmentPhoto(owner, Guid.NewGuid(), PhotoFile.Create("image/jpeg", 10));

    private static Garment WithPhoto() => Garment.Create(
        Owner,
        Classification.Create(Category.Top, Color.Blue, null),
        ImportSource.Manual,
        TestClock.Now,
        PhotoOf(Owner));

    [Fact]
    public void RemovePhoto_leaves_the_garment_without_a_photo()
    {
        var garment = WithPhoto();

        garment.RemovePhoto(TestClock.Now.AddHours(1));

        Assert.Null(garment.PhotoKey);
        Assert.Equal(TestClock.Now.AddHours(1), garment.UpdatedAt);
    }

    [Fact]
    public void An_archived_garment_keeps_its_photo()
    {
        var garment = WithPhoto();
        garment.Archive(TestClock.Now.AddHours(1));

        Assert.Throws<ArchivedGarmentIsReadOnlyException>(() => garment.RemovePhoto(TestClock.Now.AddHours(2)));
        Assert.NotNull(garment.PhotoKey);
    }

    [Fact]
    public void A_photo_of_another_user_cannot_replace_the_current_one()
    {
        var garment = WithPhoto();

        var exception = Assert.Throws<DomainValidationException>(
            () => garment.ReplacePhoto(PhotoOf(UserId.New()), TestClock.Now.AddHours(1)));

        Assert.Equal(Garment.Errors.PhotoNotOwned, exception.Code);
    }
}
