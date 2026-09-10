using System.Globalization;
using Buckl.Domain.Common;

namespace Buckl.Domain.Tests.Common;

public class MoneyTests
{
    [Fact]
    public void Create_keeps_amount_and_currency()
    {
        var money = Money.Create(1250.50m, "USD");

        Assert.Equal(1250.50m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Create_normalizes_currency_to_upper_case_and_trims_it()
    {
        var money = Money.Create(10m, " ars ");

        Assert.Equal("ARS", money.Currency);
    }

    [Fact]
    public void Create_accepts_zero()
    {
        var money = Money.Create(0m, "EUR");

        Assert.Equal(0m, money.Amount);
    }

    [Fact]
    public void Create_rejects_negative_amount()
    {
        var exception = Assert.Throws<DomainValidationException>(() => Money.Create(-0.01m, "USD"));

        Assert.Equal(Money.Errors.NegativeAmount, exception.Code);
    }

    [Theory]
    [InlineData("1.005")]
    [InlineData("0.001")]
    public void Create_rejects_more_than_two_decimals(string amount)
    {
        var value = decimal.Parse(amount, CultureInfo.InvariantCulture);

        var exception = Assert.Throws<DomainValidationException>(() => Money.Create(value, "USD"));

        Assert.Equal(Money.Errors.TooManyDecimals, exception.Code);
    }

    [Fact]
    public void Create_accepts_trailing_zero_decimals()
    {
        var money = Money.Create(1.500m, "USD");

        Assert.Equal(1.50m, money.Amount);
    }

    [Fact]
    public void Create_rejects_amount_above_maximum()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Money.Create(Money.MaxAmount + 0.01m, "USD"));

        Assert.Equal(Money.Errors.AmountTooLarge, exception.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U5D")]
    [InlineData("us-")]
    public void Create_rejects_currency_that_is_not_three_letters(string currency)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Money.Create(1m, currency));

        Assert.Equal(Money.Errors.InvalidCurrency, exception.Code);
    }

    [Fact]
    public void Create_rejects_null_currency()
    {
        Assert.Throws<ArgumentNullException>(() => Money.Create(1m, null!));
    }

    [Fact]
    public void Equality_is_by_value()
    {
        Assert.Equal(Money.Create(10m, "ARS"), Money.Create(10.00m, "ars"));
        Assert.NotEqual(Money.Create(10m, "ARS"), Money.Create(10m, "USD"));
        Assert.NotEqual(Money.Create(10m, "ARS"), Money.Create(11m, "ARS"));
    }
}
