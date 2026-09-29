using Buckl.Application.Abstractions;
using Buckl.Application.Photos;
using Buckl.Domain.Garments;

namespace Buckl.Application.Garments;

/// <summary>Edits some fields of one of the current user's garments.</summary>
public sealed class UpdateGarmentHandler
{
    private readonly IGarmentRepository _garments;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUser _currentUser;

    private readonly TimeProvider _time;

    private readonly PhotoAttacher _photos;

    public UpdateGarmentHandler(
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
        UpdateGarmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var garment = await _garments.GetOwnedAsync(command.Id, _currentUser.Id, cancellationToken);

        // Checked before anything in the command is read: an archived garment answers the same
        // whatever the request asked for, and a photo upload is never consumed for it.
        if (garment.IsArchived)
        {
            throw new ArchivedGarmentIsReadOnlyException(garment.Id);
        }

        var now = _time.GetUtcNow();

        if (command.Classification.IsSet)
        {
            garment.UpdateClassification(command.Classification.Value.ToDomain(), now);
        }

        if (command.Purchase.IsSet)
        {
            garment.UpdatePurchaseInfo(command.Purchase.Value?.ToDomain(now), now);
        }

        if (command.Notes.IsSet)
        {
            garment.UpdateNotes(command.Notes.Value, now);
        }

        var previousPhoto = garment.PhotoKey;

        if (command.Photo.IsSet)
        {
            if (command.Photo.Value is { } uploadId)
            {
                garment.ReplacePhoto(await _photos.AttachAsync(_currentUser.Id, uploadId, cancellationToken), now);
            }
            else
            {
                garment.RemovePhoto(now);
            }
        }

        await _garments.UpdateAsync(garment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousPhoto is not null && previousPhoto != garment.PhotoKey)
        {
            await _photos.ReleaseAsync(previousPhoto, cancellationToken);
        }

        return garment;
    }
}
