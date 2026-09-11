using Buckl.Domain.Garments;

namespace Buckl.Domain.Tests.Garments;

public class GarmentArchivingTests
{
    private static readonly DateTimeOffset Later = TestClock.Now.AddHours(2);

    [Fact]
    public void Archive_marks_the_garment_archived_and_stamps_archived_at_in_utc()
    {
        var garment = GarmentMother.Active();
        var when = new DateTimeOffset(2026, 9, 9, 10, 0, 0, TimeSpan.FromHours(-3));

        garment.Archive(when);

        Assert.Equal(GarmentStatus.Archived, garment.Status);
        Assert.True(garment.IsArchived);
        Assert.Equal(when.ToUniversalTime(), garment.ArchivedAt);
        Assert.Equal(when.ToUniversalTime(), garment.UpdatedAt);
        Assert.Equal(TestClock.Now, garment.CreatedAt);
    }

    [Fact]
    public void Archive_twice_throws_already_archived()
    {
        var garment = GarmentMother.Active();
        garment.Archive(TestClock.Now);

        var exception = Assert.Throws<GarmentAlreadyArchivedException>(() => garment.Archive(Later));

        Assert.Equal(GarmentAlreadyArchivedException.ErrorCode, exception.Code);
        Assert.Equal(garment.Id, exception.GarmentId);
    }

    [Fact]
    public void Restore_reactivates_the_garment_and_clears_archived_at()
    {
        var garment = GarmentMother.Active();
        garment.Archive(TestClock.Now);

        garment.Restore(Later);

        Assert.Equal(GarmentStatus.Active, garment.Status);
        Assert.Null(garment.ArchivedAt);
        Assert.Equal(Later, garment.UpdatedAt);
    }

    [Fact]
    public void Restore_on_an_active_garment_throws_not_archived()
    {
        var garment = GarmentMother.Active();

        var exception = Assert.Throws<GarmentNotArchivedException>(() => garment.Restore(Later));

        Assert.Equal(GarmentNotArchivedException.ErrorCode, exception.Code);
        Assert.Equal(garment.Id, exception.GarmentId);
    }

    [Fact]
    public void A_restored_garment_can_be_archived_again()
    {
        var garment = GarmentMother.Active();
        garment.Archive(TestClock.Now);
        garment.Restore(Later);

        garment.Archive(Later.AddHours(1));

        Assert.True(garment.IsArchived);
    }
}
