using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Classification as a request states it, before the domain validates it.</summary>
public sealed record ClassificationInput(Category Category, Color Color, string? SizeLabel)
{
    /// <summary>A blank size label means the garment has no size.</summary>
    public Classification ToDomain() => Classification.Create(
        Category,
        Color,
        string.IsNullOrWhiteSpace(SizeLabel) ? null : Size.Create(SizeLabel));
}
