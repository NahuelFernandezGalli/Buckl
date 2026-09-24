namespace Buckl.Infrastructure.Persistence.Records;

/// <summary>Row of <c>users</c>: the link between an identity provider subject and the local
/// user id that owns rows. Holds no personal data.</summary>
public sealed class UserRecord
{
    public Guid Id { get; set; }

    public string Auth0Subject { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
