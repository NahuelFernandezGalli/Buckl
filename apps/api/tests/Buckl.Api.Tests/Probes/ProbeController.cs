using Buckl.Api.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Buckl.Api.Tests.Probes;

/// <summary>Endpoints that exist only in tests, to observe the request pipeline from inside.</summary>
[ApiController]
[Route("test/probe")]
public sealed class ProbeController : ControllerBase
{
    [HttpGet("subject")]
    public ActionResult<string> Subject() => User.FindFirst(BucklClaims.Subject)?.Value ?? string.Empty;
}
