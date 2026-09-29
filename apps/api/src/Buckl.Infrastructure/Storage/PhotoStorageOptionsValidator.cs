using Microsoft.Extensions.Options;

namespace Buckl.Infrastructure.Storage;

/// <summary>Reports every problem at once, each naming its key, so a half-configured environment
/// is fixed in one go.</summary>
internal sealed class PhotoStorageOptionsValidator : IValidateOptions<PhotoStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, PhotoStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var problems = options.Problems().ToList();

        return problems.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(problems);
    }
}
