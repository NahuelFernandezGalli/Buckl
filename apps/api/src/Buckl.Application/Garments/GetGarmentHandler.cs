using Buckl.Application.Abstractions;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>One garment of the current user.</summary>
public sealed class GetGarmentHandler
{
    private readonly IGarmentRepository _garments;

    private readonly ICurrentUser _currentUser;

    public GetGarmentHandler(IGarmentRepository garments, ICurrentUser currentUser)
    {
        _garments = garments;
        _currentUser = currentUser;
    }

    public Task<Garment> HandleAsync(GarmentId id, CancellationToken cancellationToken = default) =>
        _garments.GetOwnedAsync(id, _currentUser.Id, cancellationToken);
}
