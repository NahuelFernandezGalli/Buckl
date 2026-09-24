using Buckl.Application.Abstractions;
using Buckl.Domain.Users;

namespace Buckl.Application.Tests.Fakes;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(UserId id)
    {
        Id = id;
    }

    public UserId Id { get; }
}
