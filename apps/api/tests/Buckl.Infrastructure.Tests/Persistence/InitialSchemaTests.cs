using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Infrastructure.Persistence;
using Buckl.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Buckl.Infrastructure.Tests.Persistence;

/// <summary>The database rejects what the domain rejects, even for writes that bypass the
/// application, and accepts every value the domain can produce (ADR-0016).</summary>
public class InitialSchemaTests
{
    private readonly PostgresDatabase _database;

    public InitialSchemaTests(PostgresDatabase database)
    {
        _database = database;
    }

    public static TheoryData<string> Categories => new(Texts<Category>());

    public static TheoryData<string> Colors => new(Texts<Color>());

    public static TheoryData<string> Sources => new(Texts<ImportSource>());

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void The_migrations_cover_the_whole_model()
    {
        using var context = _database.CreateOwnerContext();

        Assert.False(
            context.Database.HasPendingModelChanges(),
            "The EF model changed without a migration. Add one with dotnet-ef migrations add.");
    }

    [Theory]
    [MemberData(nameof(Categories))]
    public async Task Every_category_is_accepted(string category) =>
        await InsertForNewUserAsync(new GarmentRow { Category = category });

    [Theory]
    [MemberData(nameof(Colors))]
    public async Task Every_color_is_accepted(string color) =>
        await InsertForNewUserAsync(new GarmentRow { Color = color });

    [Theory]
    [MemberData(nameof(Sources))]
    public async Task Every_source_is_accepted(string source) =>
        await InsertForNewUserAsync(new GarmentRow { Source = source });

    [Fact]
    public async Task An_archived_garment_with_its_archive_time_is_accepted() =>
        await InsertForNewUserAsync(new GarmentRow
        {
            Status = EnumText.ToText(GarmentStatus.Archived),
            ArchivedAt = TestClock.Now.AddHours(1),
        });

    [Fact]
    public async Task A_complete_purchase_is_accepted() =>
        await InsertForNewUserAsync(new GarmentRow
        {
            PurchaseAmount = 49.90m,
            PurchaseCurrency = "USD",
            PurchaseDate = TestClock.Today,
        });

    [Fact]
    public Task An_unknown_category_is_rejected() =>
        AssertCheckViolationAsync("garments_category_values", new GarmentRow { Category = "hat" });

    [Fact]
    public Task An_archived_garment_needs_an_archive_time() =>
        AssertCheckViolationAsync(
            "garments_archived_at_matches_status",
            new GarmentRow { Status = "archived", ArchivedAt = null });

    [Fact]
    public Task Purchase_information_is_all_or_nothing() =>
        AssertCheckViolationAsync(
            "garments_purchase_all_or_none",
            new GarmentRow { PurchaseAmount = 10m });

    [Fact]
    public Task A_currency_must_be_three_upper_case_letters() =>
        AssertCheckViolationAsync(
            "garments_purchase_currency_format",
            new GarmentRow { PurchaseAmount = 10m, PurchaseCurrency = "usd", PurchaseDate = TestClock.Today });

    [Fact]
    public Task A_photo_key_must_live_under_its_owner_prefix() =>
        AssertCheckViolationAsync(
            "garments_photo_key_under_owner",
            new GarmentRow { PhotoKey = $"users/{Guid.NewGuid():D}/garments/shirt.jpg" });

    [Fact]
    public async Task A_subject_belongs_to_one_user_only()
    {
        var subject = $"seed|{Guid.NewGuid():N}";
        await _database.InsertUserAsync(subject, Ct);

        var exception = await Assert.ThrowsAsync<PostgresException>(
            () => _database.InsertUserAsync(subject, Ct));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
        Assert.Equal("users_auth0_subject_key", exception.ConstraintName);
    }

    [Fact]
    public async Task Deleting_a_user_deletes_their_garments()
    {
        var owner = await _database.InsertUserAsync(Ct);
        var garment = await _database.InsertGarmentAsync(owner, Ct);

        await _database.ExecuteAsOwnerAsync($"delete from users where id = '{owner.Value:D}'", Ct);

        Assert.False(await _database.GarmentExistsAsync(garment, Ct));
    }

    private static string[] Texts<TEnum>()
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>().Select(EnumText.ToText).ToArray();

    private async Task InsertForNewUserAsync(GarmentRow row)
    {
        var owner = await _database.InsertUserAsync(Ct);

        var id = await _database.InsertGarmentAsync(owner, row, Ct);

        Assert.True(await _database.GarmentExistsAsync(id, Ct));
    }

    private async Task AssertCheckViolationAsync(string constraint, GarmentRow row)
    {
        var owner = await _database.InsertUserAsync(Ct);

        var exception = await Assert.ThrowsAsync<PostgresException>(
            () => _database.InsertGarmentAsync(owner, row, Ct));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.Equal(constraint, exception.ConstraintName);
    }
}
