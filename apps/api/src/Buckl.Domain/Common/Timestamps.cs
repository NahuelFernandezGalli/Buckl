namespace Buckl.Domain.Common;

/// <summary>Postgres stores timestamps to the microsecond and .NET to the 100-nanosecond tick.
/// Every instant the domain records goes through <see cref="Normalize"/>, so what a command
/// returns is exactly what a later read returns.</summary>
public static class Timestamps
{
    public static DateTimeOffset Normalize(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();

        return utc.AddTicks(-(utc.Ticks % TimeSpan.TicksPerMicrosecond));
    }
}
