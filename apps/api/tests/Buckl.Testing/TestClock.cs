namespace Buckl.Testing;

/// <summary>Fixed points in time for integration and application tests. Whole seconds, so values
/// survive the round trip through Postgres, which stores microseconds.</summary>
public static class TestClock
{
    public static readonly DateTimeOffset Now = new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);

    public static readonly DateOnly Today = DateOnly.FromDateTime(Now.UtcDateTime);
}
