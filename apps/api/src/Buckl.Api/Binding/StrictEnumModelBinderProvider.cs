using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buckl.Api.Binding;

/// <summary>Uses <see cref="StrictEnumModelBinder"/> for every enumeration MVC binds from the
/// request line. Bodies are not affected: JSON has its own converter (<c>BucklJson</c>).</summary>
public sealed class StrictEnumModelBinderProvider : IModelBinderProvider
{
    private static readonly StrictEnumModelBinder Binder = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.UnderlyingOrModelType.IsEnum ? Binder : null;
    }
}
