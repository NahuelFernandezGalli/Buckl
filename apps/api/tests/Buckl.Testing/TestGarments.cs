using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Domain.Users;

namespace Buckl.Testing;

/// <summary>Valid domain garments for application and integration tests, so each test states
/// only the data that matters to it.</summary>
public static class TestGarments
{
    public static Garment Active(
        UserId owner,
        Category category = Category.Top,
        Color color = Color.Blue,
        string? size = "M",
        string? notes = null,
        ProductId? productId = null,
        PurchaseInfo? purchaseInfo = null,
        PhotoKey? photoKey = null,
        DateTimeOffset? createdAt = null) =>
        Garment.Create(
            owner,
            Classification.Create(category, color, size is null ? null : Size.Create(size)),
            ImportSource.Manual,
            createdAt ?? TestClock.Now,
            photoKey,
            purchaseInfo,
            productId,
            notes);

    public static Garment Archived(
        UserId owner,
        Category category = Category.Top,
        DateTimeOffset? createdAt = null)
    {
        var garment = Active(owner, category, createdAt: createdAt);
        garment.Archive((createdAt ?? TestClock.Now).AddHours(1));

        return garment;
    }

    public static PurchaseInfo Purchase(decimal amount = 49.90m, string currency = "USD") =>
        PurchaseInfo.Create(Money.Create(amount, currency), TestClock.Today.AddDays(-7), TestClock.Today);

    public static PhotoKey PhotoFor(UserId owner) =>
        PhotoKey.Create($"{PhotoKey.PrefixFor(owner)}garments/{Guid.NewGuid():N}.jpg", owner);
}
