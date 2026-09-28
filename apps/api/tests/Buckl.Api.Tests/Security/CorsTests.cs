using System.Net;

namespace Buckl.Api.Tests.Security;

/// <summary>Only the web app's origins may call the API from a browser. The host runs in
/// Development, whose configuration allows the Vite dev server.</summary>
public class CorsTests
{
    private const string WebApp = "http://localhost:5173";

    private const string Stranger = "https://evil.example";

    private readonly BucklApiFactory _api;

    public CorsTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_preflight_from_the_web_app_is_allowed_without_a_token()
    {
        using var client = _api.CreateClient();

        using var response = await client.SendAsync(Preflight(WebApp, "POST", "authorization,content-type"), Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(WebApp, Single(response, "Access-Control-Allow-Origin"));
        Assert.Contains("POST", Single(response, "Access-Control-Allow-Methods"), StringComparison.Ordinal);
        Assert.Contains("authorization", Single(response, "Access-Control-Allow-Headers"), StringComparison.OrdinalIgnoreCase);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
    }

    [Fact]
    public async Task A_preflight_from_another_origin_is_not_allowed()
    {
        using var client = _api.CreateClient();

        using var response = await client.SendAsync(Preflight(Stranger, "GET", "authorization"), Ct);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task A_preflight_for_a_method_the_api_does_not_serve_is_not_allowed()
    {
        using var client = _api.CreateClient();

        using var response = await client.SendAsync(Preflight(WebApp, "DELETE", "authorization"), Ct);

        // ASP.NET Core answers with the policy's methods and lets the browser refuse the rest.
        Assert.DoesNotContain("DELETE", Single(response, "Access-Control-Allow-Methods"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_response_to_the_web_app_exposes_the_location_of_a_new_garment()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        using var request = new HttpRequestMessage(HttpMethod.Post, Http.Url("/garments"))
        {
            Content = Http.Json("""{ "classification": { "category": "top", "color": "blue", "size": "M" } }"""),
        };
        request.Headers.Add("Origin", WebApp);

        using var response = await client.SendAsync(request, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(WebApp, Single(response, "Access-Control-Allow-Origin"));
        Assert.Contains("Location", Single(response, "Access-Control-Expose-Headers"), StringComparison.OrdinalIgnoreCase);
    }

    private static HttpRequestMessage Preflight(string origin, string method, string headers)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, Http.Url("/garments"));
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", method);
        request.Headers.Add("Access-Control-Request-Headers", headers);

        return request;
    }

    private static string Single(HttpResponseMessage response, string header) =>
        Assert.Single(response.Headers.GetValues(header));
}
