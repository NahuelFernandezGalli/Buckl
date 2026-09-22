using Microsoft.Extensions.DependencyInjection;

namespace Buckl.Infrastructure;

/// <summary>Registers the adapters that implement the application's ports. Called once, from the
/// API's composition root.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}
