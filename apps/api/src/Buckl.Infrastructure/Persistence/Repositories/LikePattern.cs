namespace Buckl.Infrastructure.Persistence.Repositories;

/// <summary>Builds ILIKE patterns from user text, so <c>%</c>, <c>_</c> and <c>\</c> are searched
/// literally. Queries pass <see cref="EscapeCharacter"/> explicitly.</summary>
public static class LikePattern
{
    public const string EscapeCharacter = @"\";

    public static string Escape(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }

    public static string Contains(string text) => $"%{Escape(text)}%";
}
