using Buckl.Domain.Common;

namespace Buckl.Domain.Tests.Common;

public class TimestampsTests
{
    [Fact]
    public void Normalize_drops_what_is_finer_than_a_microsecond_and_keeps_utc()
    {
        var local = new DateTimeOffset(2026, 9, 8, 9, 0, 0, TimeSpan.FromHours(-3)).AddTicks(12_345_678);

        var normalized = Timestamps.Normalize(local);

        Assert.Equal(TimeSpan.Zero, normalized.Offset);
        Assert.Equal(new DateTimeOffset(2026, 9, 8, 12, 0, 1, TimeSpan.Zero).AddTicks(2_345_670), normalized);
    }

    [Fact]
    public void Normalize_leaves_a_whole_microsecond_alone()
    {
        var instant = TestClock.Now.AddTicks(40);

        Assert.Equal(instant, Timestamps.Normalize(instant));
    }
}
