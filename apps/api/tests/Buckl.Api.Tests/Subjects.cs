namespace Buckl.Api.Tests;

internal static class Subjects
{
    /// <summary>A subject shaped like an Auth0 database user that no other test uses, so every test
    /// has its own user.</summary>
    public static string New() => $"auth0|test-{Guid.NewGuid():N}";
}
