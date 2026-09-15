namespace Buckl.Domain.Garments;

/// <summary>The kind of garment. Drives wardrobe filters and, from phase 8, outfit composition.
/// Persisted as lower-case text; the order here is not significant.</summary>
public enum Category
{
    Top,
    Bottom,
    Dress,
    Outerwear,
    Footwear,
    Accessory,
}
