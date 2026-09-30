using Buckl.Api.Contracts;
using Buckl.Application.Photos;
using Microsoft.AspNetCore.Mvc;

namespace Buckl.Api.Controllers;

/// <summary>Photo uploads of the authenticated user (ADR-0032). The photo itself never passes
/// through the API.</summary>
[ApiController]
[Route("photos")]
public sealed class PhotosController : ControllerBase
{
    [HttpPost("uploads")]
    public async Task<PhotoUploadResponse> RequestUpload(
        PhotoUploadRequest request,
        [FromServices] RequestPhotoUploadHandler handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return PhotoUploadResponse.From(await handler.HandleAsync(request.ToCommand(), cancellationToken));
    }
}
