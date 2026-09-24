using System.Net;

namespace Buckl.Api.Tests.Errors;

/// <summary>Every error leaves the API as problem details with a stable "code", so the web app
/// never matches on message text.</summary>
public class ErrorContractTests
{
    private readonly BucklApiFactory _api;

    public ErrorContractTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task An_unauthenticated_request_gets_401_with_its_code()
    {
        using var client = _api.CreateClient();

        using var response = await client.GetAsync(Http.Url("/test/probe/subject"), Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("request.unauthenticated", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task An_unknown_route_gets_404_with_its_code()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.GetAsync(Http.Url("/nowhere"), Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("resource.not_found", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task An_unexpected_failure_gets_500_with_its_code_and_no_details()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.PostAsync(
            Http.Url($"/test/probe/products/{Guid.NewGuid()}?fail=true"),
            content: null,
            Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("server.error", await response.ReadProblemCodeAsync(Ct));
        Assert.DoesNotContain("Probe failure", await response.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_openapi_document_is_served_anonymously_in_development()
    {
        using var client = _api.CreateClient();

        using var response = await client.GetAsync(Http.Url("/openapi/v1.json"), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
