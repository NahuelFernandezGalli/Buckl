namespace Buckl.Domain.Common;

/// <summary>Where a garment or a product came from. Persisted as lower-case text.</summary>
public enum ImportSource
{
    Manual,
    Url,
    Email,
}
