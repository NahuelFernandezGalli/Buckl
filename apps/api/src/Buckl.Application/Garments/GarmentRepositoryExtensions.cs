using Buckl.Domain.Garments;
using Buckl.Domain.Users;

namespace Buckl.Application.Garments;

internal static class GarmentRepositoryExtensions
{
    /// <summary>Loads a garment the given user owns, or reports it as not found. Row-Level
    /// Security already hides other users' rows; comparing the owner here is the application-level
    /// second layer (ADR-0007).</summary>
    public static async Task<Garment> GetOwnedAsync(
        this IGarmentRepository garments,
        GarmentId id,
        UserId ownerId,
        CancellationToken cancellationToken)
    {
        var garment = await garments.GetByIdAsync(id, cancellationToken);

        return garment is not null && garment.OwnerId == ownerId
            ? garment
            : throw new GarmentNotFoundException(id);
    }
}
