namespace Buckl.Infrastructure.Persistence;

/// <summary>A stored row breaks a domain rule, so it cannot become an aggregate. That is a fault
/// in the data, not in the request: nothing maps it, so the API answers 500 instead of the 400 a
/// <see cref="Buckl.Domain.Common.DomainValidationException"/> would get. The message names the
/// row and the rule, never the values.</summary>
public sealed class CorruptRecordException : Exception
{
    public CorruptRecordException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
