using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Domain.Tests.Garments;

public class PurchaseInfoTests
{
    private static readonly Money Price = Money.Create(49.99m, "USD");

    [Fact]
    public void Create_keeps_price_and_date()
    {
        var date = TestClock.Today.AddDays(-30);

        var purchase = PurchaseInfo.Create(Price, date, TestClock.Today);

        Assert.Equal(Price, purchase.Price);
        Assert.Equal(date, purchase.Date);
    }

    [Fact]
    public void Create_accepts_today()
    {
        var purchase = PurchaseInfo.Create(Price, TestClock.Today, TestClock.Today);

        Assert.Equal(TestClock.Today, purchase.Date);
    }

    [Fact]
    public void Create_rejects_a_date_in_the_future()
    {
        var tomorrow = TestClock.Today.AddDays(1);

        var exception = Assert.Throws<DomainValidationException>(
            () => PurchaseInfo.Create(Price, tomorrow, TestClock.Today));

        Assert.Equal(PurchaseInfo.Errors.DateInFuture, exception.Code);
    }

    [Fact]
    public void Create_rejects_null_price()
    {
        Assert.Throws<ArgumentNullException>(() => PurchaseInfo.Create(null!, TestClock.Today, TestClock.Today));
    }

    [Fact]
    public void Equality_is_by_value()
    {
        var a = PurchaseInfo.Create(Price, TestClock.Today, TestClock.Today);
        var b = PurchaseInfo.Create(Money.Create(49.99m, "usd"), TestClock.Today, TestClock.Today);

        Assert.Equal(a, b);
    }
}
