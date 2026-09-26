using System.Net;
using System.Text.Json;
using Buckl.Domain.Garments;
using Buckl.Testing;

namespace Buckl.Api.Tests.Isolation;

/// <summary>The exit criterion of phase 5: two users, each calling with a signed access token
/// through the production authentication, cannot see or change each other's garments. Every
/// garment is created over HTTP, as the web app will.</summary>
public class MultiUserIsolationTests
{
    private readonly BucklApiFactory _api;

    public MultiUserIsolationTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Each_user_lists_only_their_own_garments_active_or_archived()
    {
        await using var alice = await TestUser.SignInAsync(_api);
        await using var bob = await TestUser.SignInAsync(_api);
        var alicesActive = await alice.AddGarmentAsync("Alice's shirt");
        var alicesArchived = await alice.AddGarmentAsync("Alice's old coat");
        await alice.PostAsync($"/garments/{alicesArchived}/archive");
        var bobsActive = await bob.AddGarmentAsync("Bob's jeans");

        Assert.Equal([alicesActive], await alice.ListIdsAsync(""));
        Assert.Equal([alicesArchived], await alice.ListIdsAsync("?status=archived"));
        Assert.Equal([bobsActive], await bob.ListIdsAsync(""));
        Assert.Empty(await bob.ListIdsAsync("?status=archived"));
    }

    [Fact]
    public async Task Searching_never_matches_another_users_garments()
    {
        await using var alice = await TestUser.SignInAsync(_api);
        await using var bob = await TestUser.SignInAsync(_api);
        await bob.AddGarmentAsync("zebra-print scarf");

        Assert.Empty(await alice.ListIdsAsync("?q=zebra"));
        Assert.Single(await bob.ListIdsAsync("?q=zebra"));
    }

    [Theory]
    [InlineData("GET", "")]
    [InlineData("PATCH", "")]
    [InlineData("POST", "/archive")]
    [InlineData("POST", "/restore")]
    public async Task Another_users_garment_is_not_found_and_stays_untouched(string method, string suffix)
    {
        await using var alice = await TestUser.SignInAsync(_api);
        await using var bob = await TestUser.SignInAsync(_api);
        var bobsGarment = await bob.AddGarmentAsync("Bob's jeans");
        var before = await bob.GetGarmentAsync(bobsGarment);

        using var response = await alice.SendAsync(
            new HttpMethod(method),
            $"/garments/{bobsGarment}{suffix}",
            method == "PATCH" ? """{ "notes": "hijacked" }""" : null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("garment.not_found", await response.ReadProblemCodeAsync(Ct));
        Assert.Equal(before.GetRawText(), (await bob.GetGarmentAsync(bobsGarment)).GetRawText());
    }

    [Fact]
    public async Task A_garment_belongs_to_the_user_whose_token_created_it_whatever_the_body_says()
    {
        await using var alice = await TestUser.SignInAsync(_api);
        await using var bob = await TestUser.SignInAsync(_api);

        using var response = await alice.SendAsync(HttpMethod.Post, "/garments", $$"""
            {
              "userId": "{{bob.LocalId}}",
              "classification": { "category": "top", "color": "red", "size": "S" }
            }
            """);
        var id = (await response.ReadJsonAsync(Ct)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(alice.LocalId, await _api.Database.ReadGarmentOwnerAsync(new GarmentId(id), Ct));
        Assert.Empty(await bob.ListIdsAsync(""));
    }

    [Fact]
    public async Task A_leftover_development_header_cannot_switch_the_caller()
    {
        await using var alice = await TestUser.SignInAsync(_api);
        await using var bob = await TestUser.SignInAsync(_api);
        var bobsGarment = await bob.AddGarmentAsync("Bob's jeans");
        alice.Client.DefaultRequestHeaders.Add("X-Dev-User", bob.Subject);

        using var response = await alice.SendAsync(HttpMethod.Get, $"/garments/{bobsGarment}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>One signed-in user: a subject, its access token, and its local id.</summary>
    private sealed class TestUser : IAsyncDisposable
    {
        private TestUser(string subject, HttpClient client, Guid localId)
        {
            Subject = subject;
            Client = client;
            LocalId = localId;
        }

        public string Subject { get; }

        public HttpClient Client { get; }

        public Guid LocalId { get; }

        public static async Task<TestUser> SignInAsync(BucklApiFactory api)
        {
            var subject = Subjects.New();
            var client = api.CreateClientFor(subject);
            var localId = await api.ProvisionAsync(subject, Ct);

            return new TestUser(subject, client, localId.Value);
        }

        public async Task<Guid> AddGarmentAsync(string notes)
        {
            using var response = await SendAsync(HttpMethod.Post, "/garments", $$"""
                {
                  "classification": { "category": "top", "color": "blue", "size": "M" },
                  "notes": "{{notes}}"
                }
                """);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return (await response.ReadJsonAsync(Ct)).GetProperty("id").GetGuid();
        }

        public async Task PostAsync(string path)
        {
            using var response = await SendAsync(HttpMethod.Post, path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public async Task<JsonElement> GetGarmentAsync(Guid id)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/garments/{id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadJsonAsync(Ct);
        }

        public async Task<IReadOnlyList<Guid>> ListIdsAsync(string query)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/garments{query}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return [.. (await response.ReadJsonAsync(Ct)).EnumerateArray().Select(garment => garment.GetProperty("id").GetGuid())];
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string? json = null)
        {
            using var request = new HttpRequestMessage(method, Http.Url(path))
            {
                Content = json is null ? null : Http.Json(json),
            };

            return await Client.SendAsync(request, Ct);
        }

        public ValueTask DisposeAsync()
        {
            Client.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
