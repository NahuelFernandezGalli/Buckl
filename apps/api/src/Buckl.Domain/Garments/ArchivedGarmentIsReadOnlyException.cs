using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>Raised when an archived garment is edited. Restore it first.</summary>
public sealed class ArchivedGarmentIsReadOnlyException : DomainException
{
    public const string ErrorCode = "garment.archived_read_only";

    public ArchivedGarmentIsReadOnlyException(GarmentId garmentId)
        : base(ErrorCode, $"Garment {garmentId} is archived and cannot be edited; restore it first.")
    {
        GarmentId = garmentId;
    }

    public GarmentId GarmentId { get; }
}
