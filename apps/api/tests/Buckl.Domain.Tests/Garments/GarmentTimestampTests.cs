using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Garments;

/// <summary>Postgres keeps microseconds; a garment must not remember more than the database will
/// give back, or a write and the next read disagree.</summary>
public class GarmentTimestampTests
{
    private static readonly DateTimeOffset Precise = TestClock.Now.AddTicks(1_234_567);

    private static readonly DateTimeOffset Stored = TestClock.Now.AddTicks(1_234_560);

    private static Garment NewGarment(DateTimeOffset now) => Garment.Create(
        UserId.New(),
        Classification.Create(Category.Top, Color.Blue, null),
        ImportSource.Manual,
        now);

    [Fact]
    public void Create_records_its_time_to_the_microsecond()
    {
        var garment = NewGarment(Precise);

        Assert.Equal(Stored, garment.CreatedAt);
        Assert.Equal(Stored, garment.UpdatedAt);
    }

    [Fact]
    public void Edits_archiving_and_restoring_record_their_time_to_the_microsecond()
    {
        var garment = NewGarment(TestClock.Now.AddDays(-1));

        garment.UpdateNotes("Linen", Precise);
        Assert.Equal(Stored, garment.UpdatedAt);

        garment.Archive(Precise.AddHours(1));
        Assert.Equal(Stored.AddHours(1), garment.ArchivedAt);

        garment.Restore(Precise.AddHours(2));
        Assert.Equal(Stored.AddHours(2), garment.UpdatedAt);
    }
}
