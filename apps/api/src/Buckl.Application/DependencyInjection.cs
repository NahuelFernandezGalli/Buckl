using Buckl.Application.Garments;
using Buckl.Application.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Buckl.Application;

/// <summary>Registers the use case handlers. Called once, from the API's composition root.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ListWardrobeHandler>();
        services.AddScoped<GetGarmentHandler>();
        services.AddScoped<GetProductHandler>();
        services.AddScoped<CreateGarmentHandler>();
        services.AddScoped<UpdateGarmentHandler>();

        return services;
    }
}
