using Buckl.Domain.Users;

namespace Buckl.Domain.Garments;

/// <summary>Persistence port for garments, implemented in the infrastructure layer in phase 4.
/// Row-Level Security in the database is the authorization boundary; the owner argument of
/// <see cref="ListByOwnerAsync"/> is an additional application-level filter, never the only one.
/// <see cref="GetByIdAsync"/> returns null for a garment the current user cannot see, so the API
/// answers 404 without revealing that the row exists.</summary>
public interface IGarmentRepository
{
    Task<Garment?> GetByIdAsync(GarmentId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Garment>> ListByOwnerAsync(
        UserId ownerId,
        WardrobeFilter filter,
        CancellationToken cancellationToken = default);

    Task AddAsync(Garment garment, CancellationToken cancellationToken = default);

    Task UpdateAsync(Garment garment, CancellationToken cancellationToken = default);
}
