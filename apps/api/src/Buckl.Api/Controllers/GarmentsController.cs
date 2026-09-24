using Buckl.Api.Contracts;
using Buckl.Application.Garments;
using Buckl.Domain.Garments;
using Microsoft.AspNetCore.Mvc;

namespace Buckl.Api.Controllers;

/// <summary>The wardrobe of the authenticated user. Each action calls one handler; the request's
/// user and transaction are set up by <c>UserTransactionFilter</c>.</summary>
[ApiController]
[Route("garments")]
public sealed class GarmentsController : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<GarmentResponse>> List(
        [FromQuery] WardrobeQuery query,
        [FromServices] ListWardrobeHandler handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var garments = await handler.HandleAsync(query.ToFilter(), cancellationToken);

        return garments.Select(GarmentResponse.From).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<GarmentResponse> Get(
        Guid id,
        [FromServices] GetGarmentHandler handler,
        CancellationToken cancellationToken) =>
        GarmentResponse.From(await handler.HandleAsync(new GarmentId(id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<GarmentResponse>> Create(
        CreateGarmentRequest request,
        [FromServices] CreateGarmentHandler handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var garment = await handler.HandleAsync(request.ToCommand(), cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = garment.Id.Value }, GarmentResponse.From(garment));
    }

    [HttpPatch("{id:guid}")]
    public async Task<GarmentResponse> Update(
        Guid id,
        UpdateGarmentRequest request,
        [FromServices] UpdateGarmentHandler handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return GarmentResponse.From(
            await handler.HandleAsync(request.ToCommand(new GarmentId(id)), cancellationToken));
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<GarmentResponse> Archive(
        Guid id,
        [FromServices] ArchiveGarmentHandler handler,
        CancellationToken cancellationToken) =>
        GarmentResponse.From(await handler.HandleAsync(new GarmentId(id), cancellationToken));

    [HttpPost("{id:guid}/restore")]
    public async Task<GarmentResponse> Restore(
        Guid id,
        [FromServices] RestoreGarmentHandler handler,
        CancellationToken cancellationToken) =>
        GarmentResponse.From(await handler.HandleAsync(new GarmentId(id), cancellationToken));
}
