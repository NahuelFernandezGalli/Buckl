using Buckl.Domain.Users;

namespace Buckl.Application.Abstractions;

/// <summary>The local user the current request acts for. The API binds it once per request,
/// before any handler runs.</summary>
public interface ICurrentUser
{
    UserId Id { get; }
}
