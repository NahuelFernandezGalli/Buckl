using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Price and purchase date as a request states them.</summary>
public sealed record PurchaseInput(decimal Amount, string Currency, DateOnly Date)
{
    /// <param name="now">The request's current instant. Its UTC date is "today" for the rule
    /// that a purchase cannot be in the future; users west of UTC are never affected, users far
    /// east of it may have to wait a few hours to record a same-day purchase (see
    /// docs/architecture/api.md).</param>
    public PurchaseInfo ToDomain(DateTimeOffset now) => PurchaseInfo.Create(
        Money.Create(Amount, Currency),
        Date,
        DateOnly.FromDateTime(now.UtcDateTime));
}
