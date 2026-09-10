using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>The size label as the user or the store states it (<c>M</c>, <c>42</c>,
/// <c>32x32</c>). Kept as a label, not normalized across sizing systems.</summary>
public sealed record Size
{
    public const int MaxLength = 20;

    private Size(string label)
    {
        Label = label;
    }

    public string Label { get; }

    public static Size Create(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        var trimmed = label.Trim();

        if (trimmed.Length == 0)
        {
            throw new DomainValidationException(Errors.Empty, "Size label cannot be empty.");
        }

        if (trimmed.Length > MaxLength)
        {
            throw new DomainValidationException(
                Errors.TooLong,
                $"Size label cannot exceed {MaxLength} characters.");
        }

        return new Size(trimmed);
    }

    public static class Errors
    {
        public const string Empty = "size.empty";
        public const string TooLong = "size.too_long";
    }
}
