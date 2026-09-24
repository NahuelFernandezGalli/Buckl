namespace Buckl.Api.Errors;

/// <summary>The <c>code</c> extension every problem details response carries. Domain and
/// application errors bring their own code (ADR-0015); everything else gets one from its
/// status.</summary>
public static class ProblemCodes
{
    public const string Key = "code";

    public static string ForStatus(int? status) => status switch
    {
        StatusCodes.Status400BadRequest => "request.invalid",
        StatusCodes.Status401Unauthorized => "request.unauthenticated",
        StatusCodes.Status403Forbidden => "request.forbidden",
        StatusCodes.Status404NotFound => "resource.not_found",
        StatusCodes.Status405MethodNotAllowed => "request.method_not_allowed",
        StatusCodes.Status415UnsupportedMediaType => "request.unsupported_media_type",
        _ => "server.error",
    };

    /// <summary>Adds the status-derived code unless an exception handler already set a more
    /// specific one.</summary>
    public static void AddDefaultCode(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ProblemDetails.Extensions.TryAdd(Key, ForStatus(context.ProblemDetails.Status));
    }
}
