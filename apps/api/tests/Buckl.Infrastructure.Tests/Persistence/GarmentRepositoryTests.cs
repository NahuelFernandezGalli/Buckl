using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence.Repositories;
using Buckl.Testing;

namespace Buckl.Infrastructure.Tests.Persistence;

public class GarmentRepositoryTests
{
    private readonly PostgresDatabase _database;

    public GarmentRepositoryTests(PostgresDatabase database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_stored_garment_is_read_back_with_every_field()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var garment = TestGarments.Active(
            alice,
            Category.Outerwear,
            Color.Navy,
            notes: "Winter coat",
            purchaseInfo: TestGarments.Purchase(120m, "EUR"),
            photoKey: TestGarments.PhotoFor(alice));
        await StoreAsync(alice, garment);

        var restored = await GetAsync(alice, garment.Id);

        Assert.NotNull(restored);
        GarmentAssertions.Same(garment, restored);
    }

    [Fact]
    public async Task GetById_returns_null_for_a_garment_of_another_user()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        var bobsGarment = TestGarments.Active(bob);
        await StoreAsync(bob, bobsGarment);

        var seenByAlice = await GetAsync(alice, bobsGarment.Id);

        Assert.Null(seenByAlice);
    }

    [Fact]
    public async Task The_wardrobe_lists_the_owners_active_garments_newest_first()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        var older = TestGarments.Active(alice, createdAt: TestClock.Now.AddDays(-2));
        var newer = TestGarments.Active(alice, createdAt: TestClock.Now.AddDays(-1));
        var archived = TestGarments.Archived(alice);
        await StoreAsync(alice, older, newer, archived);
        await StoreAsync(bob, TestGarments.Active(bob));

        var wardrobe = await ListAsync(alice, WardrobeFilter.Default);

        Assert.Equal(new[] { newer.Id, older.Id }, wardrobe.Select(garment => garment.Id));
    }

    [Fact]
    public async Task The_wardrobe_filters_by_category_and_color()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var blueTop = TestGarments.Active(alice, Category.Top, Color.Blue);
        var blackBottom = TestGarments.Active(alice, Category.Bottom, Color.Black);
        var blueBottom = TestGarments.Active(alice, Category.Bottom, Color.Blue);
        await StoreAsync(alice, blueTop, blackBottom, blueBottom);

        var wardrobe = await ListAsync(alice, new WardrobeFilter { Category = Category.Bottom, Color = Color.Blue });

        Assert.Equal(blueBottom.Id, Assert.Single(wardrobe).Id);
    }

    [Fact]
    public async Task The_wardrobe_filters_by_size_ignoring_case()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var medium = TestGarments.Active(alice, size: "M");
        var large = TestGarments.Active(alice, size: "L");
        var noSize = TestGarments.Active(alice, size: null);
        await StoreAsync(alice, medium, large, noSize);

        var wardrobe = await ListAsync(alice, new WardrobeFilter { Size = Size.Create("m") });

        Assert.Equal(medium.Id, Assert.Single(wardrobe).Id);
    }

    [Fact]
    public async Task The_wardrobe_lists_archived_garments_when_asked()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var active = TestGarments.Active(alice);
        var archived = TestGarments.Archived(alice);
        await StoreAsync(alice, active, archived);

        var wardrobe = await ListAsync(alice, new WardrobeFilter { Status = GarmentStatus.Archived });

        Assert.Equal(archived.Id, Assert.Single(wardrobe).Id);
    }

    [Theory]
    [InlineData("wedding", "notes")]
    [InlineData("xl", "size")]
    [InlineData("oxf", "product name")]
    [InlineData("LEVI", "product brand")]
    public async Task Search_matches_notes_size_and_the_linked_product(string text, string matchingField)
    {
        var alice = await _database.InsertUserAsync(Ct);
        var product = Product.Create("Oxford shirt", ImportSource.Url, TestClock.Now, brand: "Levi's");
        var byField = new Dictionary<string, Garment>
        {
            ["notes"] = TestGarments.Active(alice, notes: "For the wedding"),
            ["size"] = TestGarments.Active(alice, size: "XL"),
            ["product name"] = TestGarments.Active(alice, productId: product.Id),
        };
        byField["product brand"] = byField["product name"];
        await StoreAsync(alice, [product], [.. byField.Values.Distinct()]);

        var wardrobe = await ListAsync(alice, new WardrobeFilter { SearchText = text });

        Assert.Equal(byField[matchingField].Id, Assert.Single(wardrobe).Id);
    }

    [Theory]
    [InlineData("BLUE", Category.Top)]
    [InlineData("bottom", Category.Bottom)]
    public async Task Search_matches_the_category_and_color(string text, Category expected)
    {
        var alice = await _database.InsertUserAsync(Ct);
        var blueTop = TestGarments.Active(alice, Category.Top, Color.Blue);
        var blackBottom = TestGarments.Active(alice, Category.Bottom, Color.Black);
        await StoreAsync(alice, blueTop, blackBottom);

        var wardrobe = await ListAsync(alice, new WardrobeFilter { SearchText = text });

        Assert.Equal(expected, Assert.Single(wardrobe).Classification.Category);
    }

    [Fact]
    public async Task Search_treats_wildcards_as_literal_text()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var percent = TestGarments.Active(alice, notes: "100% cotton");
        var digits = TestGarments.Active(alice, notes: "1000 cotton threads");
        var underscore = TestGarments.Active(alice, notes: "tag a_b");
        var letter = TestGarments.Active(alice, notes: "tag axb");
        await StoreAsync(alice, percent, digits, underscore, letter);

        var byPercent = await ListAsync(alice, new WardrobeFilter { SearchText = "100%" });
        var byUnderscore = await ListAsync(alice, new WardrobeFilter { SearchText = "a_b" });

        Assert.Equal(percent.Id, Assert.Single(byPercent).Id);
        Assert.Equal(underscore.Id, Assert.Single(byUnderscore).Id);
    }

    [Fact]
    public async Task Changes_made_through_the_aggregate_are_stored_by_update()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var garment = TestGarments.Active(alice);
        await StoreAsync(alice, garment);

        await using (var scope = await UserScope.BeginAsync(_database, alice, Ct))
        {
            var repository = new EfGarmentRepository(scope.Context);
            var loaded = await repository.GetByIdAsync(garment.Id, Ct);
            loaded!.UpdateNotes("Needs ironing", TestClock.Now.AddHours(1));
            loaded.Archive(TestClock.Now.AddHours(2));
            await repository.UpdateAsync(loaded, Ct);
            await scope.SaveAndCommitAsync(Ct);
        }

        var restored = await GetAsync(alice, garment.Id);
        Assert.Equal("Needs ironing", restored!.Notes);
        Assert.True(restored.IsArchived);
    }

    [Fact]
    public async Task Updating_a_garment_that_is_not_stored_throws()
    {
        var alice = await _database.InsertUserAsync(Ct);
        await using var scope = await UserScope.BeginAsync(_database, alice, Ct);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new EfGarmentRepository(scope.Context).UpdateAsync(TestGarments.Active(alice), Ct));
    }

    private Task StoreAsync(UserId owner, params Garment[] garments) => StoreAsync(owner, [], garments);

    private async Task StoreAsync(UserId owner, Product[] products, Garment[] garments)
    {
        await using var scope = await UserScope.BeginAsync(_database, owner, Ct);
        var productRepository = new EfProductRepository(scope.Context);
        var garmentRepository = new EfGarmentRepository(scope.Context);

        foreach (var product in products)
        {
            await productRepository.AddAsync(product, Ct);
        }

        foreach (var garment in garments)
        {
            await garmentRepository.AddAsync(garment, Ct);
        }

        await scope.SaveAndCommitAsync(Ct);
    }

    private async Task<Garment?> GetAsync(UserId reader, GarmentId id)
    {
        await using var scope = await UserScope.BeginAsync(_database, reader, Ct);

        return await new EfGarmentRepository(scope.Context).GetByIdAsync(id, Ct);
    }

    private async Task<IReadOnlyList<Garment>> ListAsync(UserId owner, WardrobeFilter filter)
    {
        await using var scope = await UserScope.BeginAsync(_database, owner, Ct);

        return await new EfGarmentRepository(scope.Context).ListByOwnerAsync(owner, filter, Ct);
    }
}
