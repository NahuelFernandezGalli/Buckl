using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Buckl.Api.Tests.Probes;
using Buckl.Testing;

namespace Buckl.Api.Tests.Filters;

public class UserTransactionFilterTests
{
    private static readonly Uri SessionProbeUrl = new("/test/probe/session", UriKind.Relative);

    private readonly BucklApiFactory _api;

    public UserTransactionFilterTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_request_runs_as_the_local_user_of_its_subject()
    {
        var subject = Subjects.New();
        using var client = _api.CreateClientFor(subject);

        var session = await client.GetFromJsonAsync<SessionProbe>(SessionProbeUrl, Ct);

        var expected = await _api.ProvisionAsync(subject, Ct);
        Assert.Equal(expected.Value, session!.UserId);
        Assert.Equal(expected.Value.ToString("D", CultureInfo.InvariantCulture), session.Setting);
    }

    [Fact]
    public async Task Two_subjects_run_as_two_different_users()
    {
        using var alice = _api.CreateClientFor(Subjects.New());
        using var bob = _api.CreateClientFor(Subjects.New());

        var alicesSession = await alice.GetFromJsonAsync<SessionProbe>(SessionProbeUrl, Ct);
        var bobsSession = await bob.GetFromJsonAsync<SessionProbe>(SessionProbeUrl, Ct);

        Assert.NotEqual(alicesSession!.UserId, bobsSession!.UserId);
    }

    [Fact]
    public async Task The_writes_of_a_successful_action_are_committed()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var productId = Guid.NewGuid();

        using var response = await client.PostAsync(
            new Uri($"/test/probe/products/{productId}?fail=false", UriKind.Relative),
            content: null,
            Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _api.Database.ProductExistsAsync(productId, Ct));
    }

    [Fact]
    public async Task The_writes_of_a_failing_action_are_rolled_back()
    {
        using var client = _api.CreateClientFor(Subjects.New());
        var productId = Guid.NewGuid();

        using var response = await client.PostAsync(
            new Uri($"/test/probe/products/{productId}?fail=true", UriKind.Relative),
            content: null,
            Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.False(await _api.Database.ProductExistsAsync(productId, Ct));
    }
}
