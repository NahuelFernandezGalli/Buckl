namespace Buckl.Domain.Common;

/// <summary>Base type for every rule the domain can reject. <see cref="Code"/> is stable and
/// machine-readable (for example <c>money.negative_amount</c>); the message is for humans.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
