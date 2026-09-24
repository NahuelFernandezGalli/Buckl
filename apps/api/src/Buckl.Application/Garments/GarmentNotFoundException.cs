using Buckl.Application.Common;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

public sealed class GarmentNotFoundException : ResourceNotFoundException
{
    public const string ErrorCode = "garment.not_found";

    public GarmentNotFoundException(GarmentId garmentId)
        : base(ErrorCode, $"Garment {garmentId} was not found.")
    {
        GarmentId = garmentId;
    }

    public GarmentId GarmentId { get; }
}
