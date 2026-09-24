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
}
