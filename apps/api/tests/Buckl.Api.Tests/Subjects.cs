namespace Buckl.Api.Tests;

internal static class Subjects
{
    /// <summary>A development subject no other test uses, so every test has its own user.</summary>
    public static string New() => $"test|{Guid.NewGuid():N}";
}
