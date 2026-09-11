namespace Buckl.Domain.Common;

/// <summary>An amount with its ISO 4217 currency code. Amounts are never handled without a
/// currency. Immutable, compared by value.</summary>
public sealed record Money
{
    public const int MaxDecimalPlaces = 2;

    /// <summary>Matches the database column <c>numeric(12,2)</c>.</summary>
    public const decimal MaxAmount = 9_999_999_999.99m;

    private const int CurrencyCodeLength = 3;

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    /// <summary>Three upper-case ASCII letters. The code format is validated; membership in the
    /// official ISO 4217 list is not, so imports can carry any code a store reports.</summary>
    public string Currency { get; }

    public static Money Create(decimal amount, string currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        if (amount < 0)
        {
            throw new DomainValidationException(Errors.NegativeAmount, "Amount cannot be negative.");
        }

        if (decimal.Round(amount, MaxDecimalPlaces) != amount)
        {
            throw new DomainValidationException(
                Errors.TooManyDecimals,
                $"Amount cannot have more than {MaxDecimalPlaces} decimal places.");
        }

        if (amount > MaxAmount)
        {
            throw new DomainValidationException(Errors.AmountTooLarge, $"Amount cannot exceed {MaxAmount}.");
        }

        return new Money(decimal.Round(amount, MaxDecimalPlaces), NormalizeCurrency(currency));
    }

    private static string NormalizeCurrency(string currency)
    {
        var code = currency.Trim().ToUpperInvariant();

        if (code.Length != CurrencyCodeLength || !code.All(char.IsAsciiLetterUpper))
        {
            throw new DomainValidationException(
                Errors.InvalidCurrency,
                "Currency must be a three-letter ISO 4217 code.");
        }

        return code;
    }

    public static class Errors
    {
        public const string NegativeAmount = "money.negative_amount";
        public const string TooManyDecimals = "money.too_many_decimals";
        public const string AmountTooLarge = "money.amount_too_large";
        public const string InvalidCurrency = "money.invalid_currency";
    }
}
