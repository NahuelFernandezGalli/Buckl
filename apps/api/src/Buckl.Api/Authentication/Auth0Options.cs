using System.ComponentModel.DataAnnotations;

namespace Buckl.Api.Authentication;

/// <summary>The Auth0 tenant and API whose access tokens the API accepts (ADR-0028). Neither value
/// is a secret: the web app ships both in its bundle. Required in every environment; the API
/// refuses to start without them.</summary>
public sealed class Auth0Options
{
    public const string SectionName = "Auth0";

    /// <summary>The tenant domain, without scheme or slashes, such as
    /// <c>buckl-dev.us.auth0.com</c>.</summary>
    [Required]
    [RegularExpression(
        "^[A-Za-z0-9]([A-Za-z0-9-]*[A-Za-z0-9])?(\\.[A-Za-z0-9]([A-Za-z0-9-]*[A-Za-z0-9])?)+$",
        ErrorMessage = "Auth0:Domain must be a host name such as buckl-dev.us.auth0.com, without https:// or slashes.")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>The identifier of the API registered in Auth0; tokens carry it as <c>aud</c>.</summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>Auth0 issues tokens with the tenant URL, trailing slash included, as <c>iss</c>.</summary>
    public string Issuer => $"https://{Domain}/";
}
