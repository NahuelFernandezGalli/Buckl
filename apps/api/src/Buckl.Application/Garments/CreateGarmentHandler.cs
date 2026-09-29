using Buckl.Application.Abstractions;
using Buckl.Application.Photos;
using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Adds a garment to the current user's wardrobe.</summary>
public sealed class CreateGarmentHandler
{
    private readonly IGarmentRepository _garments;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUser _currentUser;

    private readonly TimeProvider _time;

    private readonly PhotoAttacher _photos;

    public CreateGarmentHandler(
        IGarmentRepository garments,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        TimeProvider time,
        PhotoAttacher photos)
    {
        _garments = garments;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _time = time;
        _photos = photos;
    }

    public async Task<Garment> HandleAsync(
        CreateGarmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = _time.GetUtcNow();
        var garment = Garment.Create(
            _currentUser.Id,
            command.Classification.ToDomain(),
            ImportSource.Manual,
            now,
            purchaseInfo: command.Purchase?.ToDomain(now),
            notes: command.Notes);

        // Every rule of the garment has passed; only now is the upload moved.
        if (command.PhotoUploadId is { } uploadId)
        {
            garment.ReplacePhoto(await _photos.AttachAsync(_currentUser.Id, uploadId, cancellationToken), now);
        }

        await _garments.AddAsync(garment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return garment;
    }
}
