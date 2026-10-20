using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buckl.Api.Binding;

/// <summary>Binds an enumeration from the query string or the route by its name only, ignoring
/// case (<c>?status=archived</c>, <c>?category=BOTTOM</c>). The framework's default also accepts
/// numbers (<c>?status=1</c>) and comma-separated flags (<c>?category=top,bottom</c>), which would
/// turn a typo into a real, different filter. An absent or empty value stays unbound, so the
/// property keeps its default.</summary>
public sealed class StrictEnumModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (result == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
        var text = result.FirstValue;

        if (result.Length == 1 && string.IsNullOrEmpty(text))
        {
            return Task.CompletedTask;
        }

        var enumType = bindingContext.ModelMetadata.UnderlyingOrModelType;
        var name = result.Length == 1
            ? Enum.GetNames(enumType).FirstOrDefault(
                candidate => string.Equals(candidate, text, StringComparison.OrdinalIgnoreCase))
            : null;

        if (name is null)
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "Use exactly one of the accepted names.");

            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(Enum.Parse(enumType, name));

        return Task.CompletedTask;
    }
}
