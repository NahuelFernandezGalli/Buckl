using Buckl.Api.Authentication;
using Buckl.Application.Abstractions;
using Buckl.Infrastructure.Persistence;
using Buckl.Infrastructure.Persistence.Records;
using Buckl.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    /// <summary>Requires a role no test user has, to observe the answer to an authenticated caller
    /// that is not allowed.</summary>
    [HttpGet("forbidden")]
    [Authorize(Roles = "nobody")]
    public IActionResult Forbidden() => NoContent();

    /// <summary>Answers a client error the API has no specific code for.</summary>
    [HttpGet("unprocessable")]
    public IActionResult Unprocessable() => StatusCode(StatusCodes.Status422UnprocessableEntity);

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

    /// <summary>Queues follow-up work under <paramref name="name"/>, then fails if asked to.</summary>
    [HttpPost("after-commit/{name}")]
    public IActionResult QueueAfterCommit(string name, [FromQuery] bool fail, [FromServices] IAfterCommit afterCommit)
    {
        afterCommit.Enqueue(_ =>
        {
            AfterCommitProbe.MarkRan(name);

            return Task.CompletedTask;
        });

        return fail ? throw new InvalidOperationException("Probe failure after queueing.") : NoContent();
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

/// <summary>What the after-commit probe ran, by the name the test gave it.</summary>
public static class AfterCommitProbe
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> Ran = new();

    public static bool HasRun(string name) => Ran.ContainsKey(name);

    public static void MarkRan(string name) => Ran[name] = true;
}
