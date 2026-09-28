namespace Buckl.Api.Security;

/// <summary>The browser origins of the web app, the only ones allowed to call the API from a page.
/// An empty list allows no cross-origin calls at all.</summary>
public sealed class WebAppCorsOptions
{
    public const string SectionName = "Cors";

    public IReadOnlyList<string> AllowedOrigins { get; set; } = [];

    /// <summary>An origin is a scheme, a host and an optional port: <c>https://buckl.app</c>, never
    /// a path, a trailing slash or a wildcard.</summary>
    public static bool IsOrigin(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
        && string.Equals(value, uri.GetLeftPart(UriPartial.Authority), StringComparison.Ordinal);
}
