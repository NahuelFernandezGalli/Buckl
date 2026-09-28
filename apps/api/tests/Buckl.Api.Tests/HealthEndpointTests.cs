using System.Net;

namespace Buckl.Api.Tests;

public class HealthEndpointTests
{
    private readonly BucklApiFactory _api;

    public HealthEndpointTests(BucklApiFactory api)
    {
        _api = api;
    }

    [Fact]
    public async Task Get_health_returns_200_without_authentication()
    {
        using var client = _api.CreateClient();

        using var response = await client.GetAsync(Http.Url("/health"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
