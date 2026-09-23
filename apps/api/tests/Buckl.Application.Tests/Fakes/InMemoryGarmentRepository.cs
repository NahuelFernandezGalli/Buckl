using Buckl.Domain.Garments;
using Buckl.Domain.Users;

namespace Buckl.Application.Tests.Fakes;

/// <summary>A garment store without Row-Level Security: <see cref="GetByIdAsync"/> returns other
/// users' garments too, so tests can prove the handlers' own owner check. Listing honors owner and
/// status only; the rest of the filter semantics belong to the real repository's tests.</summary>
internal sealed class InMemoryGarmentRepository : IGarmentRepository
{
    private readonly List<Garment> _garments;

    public InMemoryGarmentRepository(params Garment[] garments)
    {
        _garments = [.. garments];
    }

    public IReadOnlyList<Garment> Stored => _garments;

    public WardrobeFilter? LastFilter { get; private set; }

    public int UpdateCount { get; private set; }

    public Task<Garment?> GetByIdAsync(GarmentId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_garments.SingleOrDefault(garment => garment.Id == id));

    public Task<IReadOnlyList<Garment>> ListByOwnerAsync(
        UserId ownerId,
        WardrobeFilter filter,
        CancellationToken cancellationToken = default)
    {
        LastFilter = filter;
        IReadOnlyList<Garment> owned = _garments
            .Where(garment => garment.OwnerId == ownerId && garment.Status == filter.Status)
            .ToList();

        return Task.FromResult(owned);
    }

    public Task AddAsync(Garment garment, CancellationToken cancellationToken = default)
    {
        _garments.Add(garment);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Garment garment, CancellationToken cancellationToken = default)
    {
        UpdateCount++;

        return Task.CompletedTask;
    }
}
