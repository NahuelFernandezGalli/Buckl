using System.Net;
using Buckl.Api.Authentication;

namespace Buckl.Api.Tests.Authentication;

public class JwtAuthenticationTests
{
    private static readonly Uri SubjectProbe = Http.Url("/test/probe/subject");

    private readonly BucklApiFactory _api;

    public JwtAuthenticationTests(BucklApiFactory api)
    {
        _api = api;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_valid_access_token_authenticates_the_request_as_its_subject()
    {
        var subject = Subjects.New();
        using var client = _api.CreateClientFor(subject);

        var body = await client.GetStringAsync(SubjectProbe, Ct);

        Assert.Equal(subject, body);
    }

    [Fact]
    public async Task A_request_without_a_token_is_challenged_for_a_bearer_token()
    {
        using var client = _api.CreateClient();

        using var response = await client.GetAsync(SubjectProbe, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains(response.Headers.WwwAuthenticate, challenge => challenge.Scheme == "Bearer");
        Assert.Equal("request.unauthenticated", await response.ReadProblemCodeAsync(Ct));
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("not yet valid")]
    [InlineData("another issuer")]
    [InlineData("another audience")]
    [InlineData("signed with a foreign key")]
    [InlineData("unsigned")]
    [InlineData("without subject")]
    [InlineData("with a subject over the limit")]
    [InlineData("not a JWT")]
    public async Task A_token_the_api_must_not_trust_is_unauthorized(string defect)
    {
        using var client = _api.CreateClientWithToken(TokenThatIs(defect));

        using var response = await client.GetAsync(SubjectProbe, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("request.unauthenticated", await response.ReadProblemCodeAsync(Ct));
    }

    [Fact]
    public async Task An_authenticated_user_without_the_required_permission_is_forbidden()
    {
        using var client = _api.CreateClientFor(Subjects.New());

        using var response = await client.GetAsync(Http.Url("/test/probe/forbidden"), Ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("request.forbidden", await response.ReadProblemCodeAsync(Ct));
    }

    private static string TokenThatIs(string defect) => defect switch
    {
        "expired" => TestTokens.Issue(new TestToken
        {
            NotBefore = DateTime.UtcNow.AddHours(-2),
            Expires = DateTime.UtcNow.AddHours(-1),
        }),
        "not yet valid" => TestTokens.Issue(new TestToken
        {
            NotBefore = DateTime.UtcNow.AddHours(1),
            Expires = DateTime.UtcNow.AddHours(2),
        }),
        "another issuer" => TestTokens.Issue(new TestToken { Issuer = "https://evil.example/" }),
        "another audience" => TestTokens.Issue(new TestToken { Audience = "https://api.other.example" }),
        "signed with a foreign key" => TestTokens.Issue(new TestToken { Key = TestTokens.ForeignKey }),
        "unsigned" => TestTokens.Issue(new TestToken { Key = null }),
        "without subject" => TestTokens.Issue(new TestToken { Subject = null }),
        "with a subject over the limit" => TestTokens.Issue(new TestToken
        {
            Subject = new string('x', AuthenticationSetup.MaxSubjectLength + 1),
        }),
        "not a JWT" => "not-a-jwt",
        _ => throw new ArgumentOutOfRangeException(nameof(defect), defect, "Unknown token defect."),
    };
}
