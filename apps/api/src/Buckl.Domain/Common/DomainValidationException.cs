namespace Buckl.Domain.Common;

/// <summary>Raised when input to a factory or a value object does not satisfy its rules.</summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string code, string message)
        : base(code, message)
    {
    }
}
