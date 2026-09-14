using Buckl.Domain.Common;

namespace Buckl.Domain.Products;

/// <summary>Identity of a catalog product. A struct wrapping a GUID so identifiers of
/// different aggregates cannot be mixed up. <c>default(ProductId)</c> bypasses the constructor and
/// is not a valid identity; always use <see cref="New"/> or the constructor.</summary>
public readonly record struct ProductId
{
    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainValidationException(Errors.Empty, "Product id cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }

    public static ProductId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static class Errors
    {
        public const string Empty = "product_id.empty";
    }
}
