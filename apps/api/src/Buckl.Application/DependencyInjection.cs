using Microsoft.Extensions.DependencyInjection;

namespace Buckl.Application;

/// <summary>Registers the use case handlers. Called once, from the API's composition root.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
