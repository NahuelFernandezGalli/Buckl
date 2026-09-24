using Buckl.Domain.Garments;
using Microsoft.AspNetCore.Mvc;

namespace Buckl.Api.Contracts;

/// <summary>Query string of <c>GET /garments</c>, named like the web app's address bar
/// (<c>?category=&amp;color=&amp;size=&amp;q=&amp;status=</c>). Enumeration values are matched
/// ignoring case; lengths are the domain's to check, so the error carries the domain's
/// code.</summary>
public sealed class WardrobeQuery
{
    public Category? Category { get; init; }

    public Color? Color { get; init; }

    [FromQuery(Name = "size")]
    public string? SizeLabel { get; init; }

    [FromQuery(Name = "q")]
    public string? SearchText { get; init; }

    public GarmentStatus Status { get; init; } = GarmentStatus.Active;

    public WardrobeFilter ToFilter() => new()
    {
        Category = Category,
        Color = Color,
        Size = string.IsNullOrWhiteSpace(SizeLabel) ? null : Size.Create(SizeLabel),
        SearchText = SearchText,
        Status = Status,
    };
}
