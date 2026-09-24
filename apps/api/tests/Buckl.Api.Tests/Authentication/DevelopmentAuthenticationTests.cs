using System.Net;
using Buckl.Api.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Buckl.Api.Tests.Authentication;

public class DevelopmentAuthenticationTests
{
    private static readonly Uri SubjectProbe = new("/test/probe/subject", UriKind.Relative);

    private readonly BucklApiFactory _api;

    public DevelopmentAuthenticationTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_request_without_the_development_header_is_unauthorized()
    {
        using var client = _api.CreateClient();

        using var response = await client.GetAsync(SubjectProbe, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task The_development_header_authenticates_the_request_as_that_subject()
    {
        var subject = Subjects.New();
        using var client = _api.CreateClientFor(subject);

        var body = await client.GetStringAsync(SubjectProbe, Ct);

        Assert.Equal(subject, body);
    }

    [Fact]
    public async Task A_subject_longer_than_the_limit_is_unauthorized()
    {
        using var client = _api.CreateClientFor(new string('x', DevelopmentAuthenticationHandler.MaxSubjectLength + 1));

        using var response = await client.GetAsync(SubjectProbe, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void The_api_refuses_to_start_outside_development()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment(Environments.Production));

        var exception = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(exception);
        Assert.Contains("phase 5", exception.ToString(), StringComparison.Ordinal);
    }
}
