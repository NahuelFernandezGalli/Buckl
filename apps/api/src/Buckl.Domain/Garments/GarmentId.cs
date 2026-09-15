using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>Identity of a garment. A struct wrapping a GUID so identifiers of different aggregates
/// cannot be mixed up. <c>default(GarmentId)</c> bypasses the constructor and is not a valid
/// identity.</summary>
public readonly record struct GarmentId
{
    public GarmentId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainValidationException(Errors.Empty, "Garment id cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }

    public static GarmentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static class Errors
    {
        public const string Empty = "garment_id.empty";
    }
}
