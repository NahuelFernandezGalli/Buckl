namespace Buckl.Api.Authentication;

public static class BucklClaims
{
    /// <summary>The identity provider's subject, as the Auth0 access token names it. JWT bearer
    /// keeps claim names as issued (<c>MapInboundClaims = false</c>).</summary>
    public const string Subject = "sub";
}
