using Buckl.Application.Abstractions;
using Buckl.Domain.Users;

namespace Buckl.Api.Authentication;

/// <summary>The request-scoped <see cref="ICurrentUser"/>. <c>UserTransactionFilter</c> binds it
/// before the action runs; reading it earlier is a programming error.</summary>
public sealed class RequestUser : ICurrentUser
{
    private UserId? _id;

    public UserId Id => _id ?? throw new InvalidOperationException(
        "No user is bound to this request. Is the endpoint behind UserTransactionFilter?");

    public void Bind(UserId id)
    {
        if (_id is { } bound && bound != id)
        {
            throw new InvalidOperationException("The request is already bound to another user.");
        }

        _id = id;
    }
}
