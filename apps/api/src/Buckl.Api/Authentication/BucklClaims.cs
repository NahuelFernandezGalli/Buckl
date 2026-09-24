namespace Buckl.Api.Authentication;

public static class BucklClaims
{
    /// <summary>The identity provider's subject. Phase 5 keeps the JWT claim name as is
    /// (<c>MapInboundClaims = false</c>), so the development scheme uses the same one.</summary>
    public const string Subject = "sub";
}
