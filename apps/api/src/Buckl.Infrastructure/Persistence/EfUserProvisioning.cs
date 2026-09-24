using Buckl.Application.Abstractions;
using Buckl.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Persistence;

/// <summary>Upserts <c>users</c> by subject. <c>on conflict do nothing</c> makes concurrent first
/// requests safe; the application role has both the insert privilege for the upsert and the
/// select privilege the follow-up read needs.</summary>
public sealed class EfUserProvisioning : IUserProvisioning
{
    private readonly BucklDbContext _context;

    private readonly TimeProvider _time;

    public EfUserProvisioning(BucklDbContext context, TimeProvider time)
    {
        _context = context;
        _time = time;
    }

    public async Task<UserId> EnsureUserAsync(string subject, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        var candidateId = Guid.NewGuid();
        var now = _time.GetUtcNow();

        await _context.Database.ExecuteSqlAsync(
            $"""
            insert into users (id, auth0_subject, created_at)
            values ({candidateId}, {subject}, {now})
            on conflict (auth0_subject) do nothing
            """,
            cancellationToken);

        var id = await _context.Users
            .AsNoTracking()
            .Where(user => user.Auth0Subject == subject)
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);

        return new UserId(id);
    }
}
