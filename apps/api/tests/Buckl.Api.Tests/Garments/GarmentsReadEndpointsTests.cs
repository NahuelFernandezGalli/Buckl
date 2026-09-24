using System.Net;
using System.Text.Json;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Testing;

namespace Buckl.Api.Tests.Garments;

public class GarmentsReadEndpointsTests
{
    private readonly BucklApiFactory _api;

    public GarmentsReadEndpointsTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_wardrobe_lists_the_callers_active_garments_newest_first_in_the_web_contract()
    {
        var subject = Subjects.New();
        var owner = await _api.ProvisionAsync(subject, Ct);
        var older = TestGarments.Active(
            owner,
            size: "M",
            notes: "Oxford shirt",
            purchaseInfo: TestGarments.Purchase(49.90m, "USD"),
            createdAt: TestClock.Now.AddDays(-2));
        var newer = TestGarments.Active(owner, Category.Bottom, Color.Black, createdAt: TestClock.Now.AddDays(-1));
        await _api.SeedAsync(owner, [older, newer, TestGarments.Archived(owner)], Ct);
        var stranger = await _api.ProvisionAsync(Subjects.New(), Ct);
        await _api.SeedAsync(stranger, [TestGarments.Active(stranger)], Ct);
        using var client = _api.CreateClientFor(subject);

        using var response = await client.GetAsync(Http.Url("/garments"), Ct);
        var wardrobe = await response.ReadJsonAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, wardrobe.GetArrayLength());
        Assert.Equal(newer.Id.Value, wardrobe[0].GetProperty("id").GetGuid());
        var first = wardrobe[1];
        Assert.Equal(older.Id.Value, first.GetProperty("id").GetGuid());
        Assert.Equal(JsonValueKind.Null, first.GetProperty("photoUrl").ValueKind);
        Assert.Equal(JsonValueKind.Null, first.GetProperty("productId").ValueKind);
        Assert.Equal("top", first.GetProperty("classification").GetProperty("category").GetString());
        Assert.Equal("blue", first.GetProperty("classification").GetProperty("color").GetString());
        Assert.Equal("M", first.GetProperty("classification").GetProperty("size").GetString());
        Assert.Equal(49.90m, first.GetProperty("purchaseInfo").GetProperty("price").GetProperty("amount").GetDecimal());
        Assert.Equal("USD", first.GetProperty("purchaseInfo").GetProperty("price").GetProperty("currency").GetString());
        Assert.Equal("2026-09-01", first.GetProperty("purchaseInfo").GetProperty("date").GetString());
        Assert.Equal("manual", first.GetProperty("source").GetString());
        Assert.Equal("active", first.GetProperty("status").GetString());
        Assert.Equal("Oxford shirt", first.GetProperty("notes").GetString());
        Assert.Equal(TestClock.Now.AddDays(-2), first.GetProperty("createdAt").GetDateTimeOffset());
        Assert.Equal(JsonValueKind.Null, first.GetProperty("archivedAt").ValueKind);
    }

    [Theory]
    [InlineData("?category=bottom&color=black", "bottom")]
    [InlineData("?category=BOTTOM", "bottom")]
    [InlineData("?status=archived", "archived")]
    [InlineData("?size=l", "large")]
    [InlineData("?q=WEDDING", "wedding")]
    public async Task The_wardrobe_applies_the_query_string_filters(string query, string expected)
    {
        var subject = Subjects.New();
        var owner = await _api.ProvisionAsync(subject, Ct);
        var garments = new Dictionary<string, Garment>
        {
            ["bottom"] = TestGarments.Active(owner, Category.Bottom, Color.Black, size: "32"),
            ["archived"] = TestGarments.Archived(owner),
            ["large"] = TestGarments.Active(owner, size: "L"),
            ["wedding"] = TestGarments.Active(owner, notes: "For the wedding"),
        };
        await _api.SeedAsync(owner, garments.Values, Ct);
        using var client = _api.CreateClientFor(subject);

        using var response = await client.GetAsync(Http.Url($"/garments{query}"), Ct);
        var wardrobe = await response.ReadJsonAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(garments[expected].Id.Value, Assert.Single(wardrobe.EnumerateArray()).GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task An_unknown_filter_value_is_a_bad_request()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.GetAsync(Http.Url("/garments?category=hat"), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("request.invalid", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task Search_text_over_the_limit_is_rejected_with_the_domain_code()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var tooLong = new string('x', WardrobeFilter.MaxSearchTextLength + 1);

        using var response = await client.GetAsync(Http.Url($"/garments?q={tooLong}"), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(WardrobeFilter.Errors.SearchTextTooLong, await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task A_garment_of_the_caller_is_returned_by_id()
    {
        var subject = Subjects.New();
        var owner = await _api.ProvisionAsync(subject, Ct);
        var garment = TestGarments.Active(owner);
        await _api.SeedAsync(owner, [garment], Ct);
        using var client = _api.CreateClientFor(subject);

        using var response = await client.GetAsync(Http.Url($"/garments/{garment.Id}"), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(garment.Id.Value, (await response.ReadJsonAsync(Ct)).GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Another_users_garment_is_not_found()
    {
        var bob = await _api.ProvisionAsync(Subjects.New(), Ct);
        var bobsGarment = TestGarments.Active(bob);
        await _api.SeedAsync(bob, [bobsGarment], Ct);
        using var alice = _api.CreateClientFor(Subjects.New());

        using var response = await alice.GetAsync(Http.Url($"/garments/{bobsGarment.Id}"), Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("garment.not_found", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task A_product_is_returned_by_id()
    {
        var product = Product.Create(
            "Oxford shirt",
            ImportSource.Url,
            TestClock.Now,
            brand: "Uniqlo",
            sourceUrl: new Uri($"https://example.com/{Guid.NewGuid():N}"));
        await _api.SeedProductAsync(product, Ct);
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.GetAsync(Http.Url($"/products/{product.Id}"), Ct);
        var body = await response.ReadJsonAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Oxford shirt", body.GetProperty("name").GetString());
        Assert.Equal("url", body.GetProperty("source").GetString());
        Assert.Equal(product.SourceUrl!.AbsoluteUri, body.GetProperty("sourceUrl").GetString());
    }

    [Fact]
    public async Task An_unknown_product_is_not_found()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.GetAsync(Http.Url($"/products/{Guid.NewGuid()}"), Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("product.not_found", await response.ReadProblemCodeAsync(Ct));
    }
}
