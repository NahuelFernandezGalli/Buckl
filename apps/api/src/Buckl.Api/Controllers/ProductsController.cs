using Buckl.Api.Contracts;
using Buckl.Application.Products;
using Buckl.Domain.Products;
using Microsoft.AspNetCore.Mvc;

namespace Buckl.Api.Controllers;

[ApiController]
[Route("products")]
public sealed class ProductsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ProductResponse> Get(
        Guid id,
        [FromServices] GetProductHandler handler,
        CancellationToken cancellationToken) =>
        ProductResponse.From(await handler.HandleAsync(new ProductId(id), cancellationToken));
}
