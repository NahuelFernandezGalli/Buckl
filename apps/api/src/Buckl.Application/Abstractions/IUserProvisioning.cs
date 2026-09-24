using Buckl.Domain.Users;

namespace Buckl.Application.Abstractions;

/// <summary>Maps an identity provider subject to the local user id, creating the user the first
/// time the subject is seen. Idempotent and safe under concurrent first requests.</summary>
public interface IUserProvisioning
{
    Task<UserId> EnsureUserAsync(string subject, CancellationToken cancellationToken = default);
}
