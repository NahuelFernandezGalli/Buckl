namespace Buckl.Application.Common;

/// <summary>The requested resource does not exist for the current user: it was never created, or
/// Row-Level Security hides it. The API answers 404 for both, so it never reveals that another
/// user's row exists. Carries a stable code, like domain exceptions (ADR-0015).</summary>
public abstract class ResourceNotFoundException : Exception
{
    protected ResourceNotFoundException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
