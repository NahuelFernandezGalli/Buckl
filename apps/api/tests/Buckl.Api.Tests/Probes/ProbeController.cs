using Buckl.Api.Authentication;
using Buckl.Application.Abstractions;
using Buckl.Infrastructure.Persistence;
using Buckl.Infrastructure.Persistence.Records;
using Buckl.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Buckl.Api.Tests.Probes;

/// <summary>Endpoints that exist only in tests, to observe the request pipeline from inside.</summary>
[ApiController]
[Route("test/probe")]
public sealed class ProbeController : ControllerBase
{
    [HttpGet("subject")]
    public ActionResult<string> Subject() => User.FindFirst(BucklClaims.Subject)?.Value ?? string.Empty;

    /// <summary>The user the request is bound to, and the session variable the policies read.</summary>
    [HttpGet("session")]
    public async Task<SessionProbe> Session(
        [FromServices] BucklDbContext context,
        [FromServices] ICurrentUser currentUser)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "select current_setting('app.user_id', true)";
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();

        var setting = await command.ExecuteScalarAsync(cancellationToken) as string;

        return new SessionProbe(currentUser.Id.Value, setting);
    }

    /// <summary>Writes a product, then fails if asked to, after the write reached the database.</summary>
    [HttpPost("products/{id:guid}")]
    public async Task<IActionResult> WriteProduct(
        Guid id,
        [FromQuery] bool fail,
        [FromServices] BucklDbContext context,
        CancellationToken cancellationToken)
    {
        context.Products.Add(new ProductRecord
        {
            Id = id,
            Name = "Probe",
            Source = "manual",
            CreatedAt = TestClock.Now,
        });
        await context.SaveChangesAsync(cancellationToken);

        return fail ? throw new InvalidOperationException("Probe failure after a write.") : NoContent();
    }
}

public sealed record SessionProbe(Guid UserId, string? Setting);
