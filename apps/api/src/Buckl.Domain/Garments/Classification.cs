using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>How a garment is classified: category, dominant color and an optional size
/// label.</summary>
public sealed record Classification
{
    private Classification(Category category, Color color, Size? size)
    {
        Category = category;
        Color = color;
        Size = size;
    }

    public Category Category { get; }

    public Color Color { get; }

    public Size? Size { get; }

    public static Classification Create(Category category, Color color, Size? size = null)
    {
        if (!Enum.IsDefined(category))
        {
            throw new DomainValidationException(
                Errors.UnknownCategory,
                $"Unknown category '{category}'.");
        }

        if (!Enum.IsDefined(color))
        {
            throw new DomainValidationException(Errors.UnknownColor, $"Unknown color '{color}'.");
        }

        return new Classification(category, color, size);
    }

    public static class Errors
    {
        public const string UnknownCategory = "classification.unknown_category";
        public const string UnknownColor = "classification.unknown_color";
    }
}
