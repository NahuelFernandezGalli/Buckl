using Buckl.Application.Garments;
using Buckl.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Buckl.Application;

/// <summary>Registers the use case handlers. Called once, from the API's composition root.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ListWardrobeHandler>();
        services.AddScoped<GetGarmentHandler>();
        services.AddScoped<GetProductHandler>();

        return services;
    }
}
