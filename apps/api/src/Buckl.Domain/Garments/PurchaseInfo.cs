using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>When and for how much a garment was bought.</summary>
public sealed record PurchaseInfo
{
    private PurchaseInfo(Money price, DateOnly date)
    {
        Price = price;
        Date = date;
    }

    public Money Price { get; }

    public DateOnly Date { get; }

    /// <param name="price">What was paid.</param>
    /// <param name="date">When it was bought.</param>
    /// <param name="today">The caller's current date; the domain never reads the clock itself.</param>
    public static PurchaseInfo Create(Money price, DateOnly date, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(price);

        if (date > today)
        {
            throw new DomainValidationException(Errors.DateInFuture, "Purchase date cannot be in the future.");
        }

        return new PurchaseInfo(price, date);
    }

    /// <summary>Rebuilds purchase information that was validated when it was written. The
    /// "not in the future" rule is a write-time rule, so it is not checked again.</summary>
    public static PurchaseInfo Rehydrate(Money price, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(price);

        return new PurchaseInfo(price, date);
    }

    public static class Errors
    {
        public const string DateInFuture = "purchase_info.date_in_future";
    }
}
