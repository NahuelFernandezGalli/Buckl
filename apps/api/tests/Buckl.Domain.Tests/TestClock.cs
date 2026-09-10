namespace Buckl.Domain.Tests;

/// <summary>Fixed points in time for the whole suite. The domain never reads the clock, so tests
/// never need the real one either.</summary>
internal static class TestClock
{
    public static readonly DateTimeOffset Now = new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);

    public static readonly DateOnly Today = DateOnly.FromDateTime(Now.UtcDateTime);
}
