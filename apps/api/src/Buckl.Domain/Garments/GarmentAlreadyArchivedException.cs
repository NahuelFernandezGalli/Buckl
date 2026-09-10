using Buckl.Domain.Common;

namespace Buckl.Domain.Garments;

/// <summary>Raised when a garment is in the wrong state for the requested transition.</summary>
public sealed class GarmentAlreadyArchivedException : DomainException
{
    public const string ErrorCode = "garment.already_archived";

    public GarmentAlreadyArchivedException(GarmentId garmentId)
        : base(ErrorCode, $"Garment {garmentId} is already archived.")
    {
        GarmentId = garmentId;
    }

    public GarmentId GarmentId { get; }
}
