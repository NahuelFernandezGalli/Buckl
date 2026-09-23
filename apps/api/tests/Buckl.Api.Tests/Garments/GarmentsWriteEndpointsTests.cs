using System.Net;
using System.Text.Json;
using Buckl.Domain.Garments;
using Buckl.Testing;

namespace Buckl.Api.Tests.Garments;

public class GarmentsWriteEndpointsTests
{
    private const string OxfordShirt = """
        {
          "classification": { "category": "top", "color": "blue", "size": "M" },
          "purchaseInfo": { "price": { "amount": 49.90, "currency": "usd" }, "date": "2026-09-01" },
          "notes": "  Oxford shirt  "
        }
        """;

    private readonly BucklApiFactory _api;

    public GarmentsWriteEndpointsTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Adding_a_garment_returns_201_with_its_location_and_the_stored_garment()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.PostAsync(Http.Url("/garments"), Http.Json(OxfordShirt), Ct);
        var body = await response.ReadJsonAsync(Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = body.GetProperty("id").GetGuid();
        Assert.Equal($"/garments/{id}", response.Headers.Location?.AbsolutePath);
        Assert.Equal("Oxford shirt", body.GetProperty("notes").GetString());
        Assert.Equal("USD", body.GetProperty("purchaseInfo").GetProperty("price").GetProperty("currency").GetString());
        Assert.Equal("manual", body.GetProperty("source").GetString());
        Assert.Equal(TestClock.Now, body.GetProperty("createdAt").GetDateTimeOffset());

        using var stored = await client.GetAsync(Http.Url($"/garments/{id}"), Ct);
        Assert.Equal(HttpStatusCode.OK, stored.StatusCode);
    }

    [Theory]
    [InlineData("""{ "notes": "no classification" }""")]
    [InlineData("""{ "classification": { "category": "hat", "color": "blue" } }""")]
    [InlineData("""{ "classification": { "color": "blue" } }""")]
    [InlineData("""{ "classification": { "category": "top", "color": "blue" }, "purchaseInfo": { "date": "2026-09-01" } }""")]
    public async Task A_malformed_garment_is_a_bad_request(string json)
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.PostAsync(Http.Url("/garments"), Http.Json(json), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("request.invalid", await response.ReadProblemCodeAsync(Ct));
    }

    [Theory]
    [InlineData("""{ "amount": 10, "currency": "USD" }""", "2026-09-09", "purchase_info.date_in_future")]
    [InlineData("""{ "amount": -1, "currency": "USD" }""", "2026-09-01", "money.negative_amount")]
    [InlineData("""{ "amount": 10, "currency": "dollars" }""", "2026-09-01", "money.invalid_currency")]
    public async Task A_purchase_the_domain_rejects_is_a_bad_request_with_the_domain_code(
        string price,
        string date,
        string code)
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var json = $$"""
            {
              "classification": { "category": "top", "color": "blue" },
              "purchaseInfo": { "price": {{price}}, "date": "{{date}}" }
            }
            """;

        using var response = await client.PostAsync(Http.Url("/garments"), Http.Json(json), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(code, await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task Patching_only_the_notes_keeps_everything_else()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var id = await AddOxfordShirtAsync(client);

        using var response = await client.PatchAsync(
            Http.Url($"/garments/{id}"),
            Http.Json("""{ "notes": "Needs ironing" }"""),
            Ct);
        var body = await response.ReadJsonAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Needs ironing", body.GetProperty("notes").GetString());
        Assert.Equal("top", body.GetProperty("classification").GetProperty("category").GetString());
        Assert.Equal(JsonValueKind.Object, body.GetProperty("purchaseInfo").ValueKind);
    }

    [Fact]
    public async Task Patching_the_purchase_to_null_clears_it()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var id = await AddOxfordShirtAsync(client);

        using var response = await client.PatchAsync(
            Http.Url($"/garments/{id}"),
            Http.Json("""{ "purchaseInfo": null }"""),
            Ct);

        var body = await response.ReadJsonAsync(Ct);

        Assert.Equal(JsonValueKind.Null, body.GetProperty("purchaseInfo").ValueKind);
        Assert.Equal("Oxford shirt", body.GetProperty("notes").GetString());
    }

    [Fact]
    public async Task Patching_the_classification_to_null_is_a_bad_request()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var id = await AddOxfordShirtAsync(client);

        using var response = await client.PatchAsync(
            Http.Url($"/garments/{id}"),
            Http.Json("""{ "classification": null }"""),
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("request.invalid", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task Archive_and_restore_move_the_garment_between_the_two_lists()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var id = await AddOxfordShirtAsync(client);

        using var archived = await client.PostAsync(Http.Url($"/garments/{id}/archive"), content: null, Ct);
        Assert.Equal("archived", (await archived.ReadJsonAsync(Ct)).GetProperty("status").GetString());
        Assert.Equal(0, (await ListAsync(client, "")).GetArrayLength());
        Assert.Equal(1, (await ListAsync(client, "?status=archived")).GetArrayLength());

        using var restored = await client.PostAsync(Http.Url($"/garments/{id}/restore"), content: null, Ct);
        Assert.Equal("active", (await restored.ReadJsonAsync(Ct)).GetProperty("status").GetString());
        Assert.Equal(1, (await ListAsync(client, "")).GetArrayLength());
    }

    [Theory]
    [InlineData("archive", "patch", "garment.archived_read_only")]
    [InlineData("archive", "archive", "garment.already_archived")]
    [InlineData(null, "restore", "garment.not_archived")]
    public async Task A_change_the_current_state_forbids_is_a_conflict(string? before, string action, string code)
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var id = await AddOxfordShirtAsync(client);
        if (before is not null)
        {
            using var setup = await client.PostAsync(Http.Url($"/garments/{id}/{before}"), content: null, Ct);
        }

        using var response = action == "patch"
            ? await client.PatchAsync(Http.Url($"/garments/{id}"), Http.Json("""{ "notes": "x" }"""), Ct)
            : await client.PostAsync(Http.Url($"/garments/{id}/{action}"), content: null, Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(code, await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task One_user_can_neither_read_nor_change_another_users_garment()
    {
        using var alice = _api.CreateClientFor(Subjects.New());
        using var bob = _api.CreateClientFor(Subjects.New());
        var alicesGarment = await AddOxfordShirtAsync(alice);

        using var read = await bob.GetAsync(Http.Url($"/garments/{alicesGarment}"), Ct);
        using var patch = await bob.PatchAsync(
            Http.Url($"/garments/{alicesGarment}"),
            Http.Json("""{ "notes": "hijacked" }"""),
            Ct);
        using var archive = await bob.PostAsync(Http.Url($"/garments/{alicesGarment}/archive"), content: null, Ct);
        using var restore = await bob.PostAsync(Http.Url($"/garments/{alicesGarment}/restore"), content: null, Ct);
        var bobsWardrobe = await ListAsync(bob, "");

        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, archive.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, restore.StatusCode);
        Assert.Equal(0, bobsWardrobe.GetArrayLength());
        Assert.Equal("Oxford shirt", await _api.Database.ReadGarmentNotesAsync(new GarmentId(alicesGarment), Ct));
        Assert.Equal(1, (await ListAsync(alice, "")).GetArrayLength());
    }

    private static async Task<Guid> AddOxfordShirtAsync(HttpClient client)
    {
        using var response = await client.PostAsync(Http.Url("/garments"), Http.Json(OxfordShirt), Ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await response.ReadJsonAsync(Ct)).GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> ListAsync(HttpClient client, string query)
    {
        using var response = await client.GetAsync(Http.Url($"/garments{query}"), Ct);

        return await response.ReadJsonAsync(Ct);
    }
}
