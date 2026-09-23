using Buckl.Application.Abstractions;
using Buckl.Domain.Users;

namespace Buckl.Api;

/// <summary>Temporary stub implementation of ICurrentUser. The real implementation (using Development auth or JWT)
/// will replace this in PR 4.11.</summary>
internal sealed class StubCurrentUser : ICurrentUser
{
    public UserId Id => throw new InvalidOperationException(
        "ICurrentUser is not yet implemented. This stub will be replaced in PR 4.11 with the Development and JWT authentication schemes.");
}
