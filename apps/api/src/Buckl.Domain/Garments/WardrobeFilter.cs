using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>Criteria for listing a user's wardrobe. Not an entity: a description of a query that
/// the repository translates. The owner is not part of the filter; it is a separate argument, and
/// Row-Level Security enforces it regardless.</summary>
public sealed record WardrobeFilter
{
    public const int MaxSearchTextLength = 100;

    private readonly string? _searchText;

    /// <summary>Active garments, no other criteria.</summary>
    public static WardrobeFilter Default { get; } = new();

    public Category? Category { get; init; }

    public Color? Color { get; init; }

    public Size? Size { get; init; }

    /// <summary>Free text the repository matches against notes, size label and the linked
    /// product's name and brand. Trimmed; blank means no text filter.</summary>
    public string? SearchText
    {
        get => _searchText;
        init => _searchText = NormalizeSearchText(value);
    }

    public GarmentStatus Status { get; init; } = GarmentStatus.Active;

    /// <summary>Whether anything narrows the list. Status alone does not count: every listing has
    /// one.</summary>
    public bool HasCriteria =>
        Category is not null || Color is not null || Size is not null || SearchText is not null;

    private static string? NormalizeSearchText(string? value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > MaxSearchTextLength)
        {
            throw new DomainValidationException(
                Errors.SearchTextTooLong,
                $"Search text cannot exceed {MaxSearchTextLength} characters.");
        }

        return trimmed;
    }

    public static class Errors
    {
        public const string SearchTextTooLong = "wardrobe_filter.search_text_too_long";
    }
}
