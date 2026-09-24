using Buckl.Application.Abstractions;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Lists the current user's wardrobe with the given filter.</summary>
public sealed class ListWardrobeHandler
{
    private readonly IGarmentRepository _garments;

    private readonly ICurrentUser _currentUser;

    public ListWardrobeHandler(IGarmentRepository garments, ICurrentUser currentUser)
    {
        _garments = garments;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<Garment>> HandleAsync(
        WardrobeFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return _garments.ListByOwnerAsync(_currentUser.Id, filter, cancellationToken);
    }
}
