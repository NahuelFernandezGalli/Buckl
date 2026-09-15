using Buckl.Domain.Common;

namespace Buckl.Domain.Users;

/// <summary>Local identity of a user (the <c>users.id</c> column), owner of garments. Never the
/// Auth0 subject. <c>default(UserId)</c> bypasses the constructor and is not a valid
/// identity.</summary>
public readonly record struct UserId
{
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainValidationException(Errors.Empty, "User id cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }

    public static UserId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static class Errors
    {
        public const string Empty = "user_id.empty";
    }
}
