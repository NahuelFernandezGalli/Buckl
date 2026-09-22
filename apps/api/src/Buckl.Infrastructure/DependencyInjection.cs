using Buckl.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Buckl.Infrastructure;

/// <summary>Registers the adapters that implement the application's ports. Called once, from the
/// API's composition root.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // The connection string is read when a context is first built, not at startup, so the
        // health endpoint and tests that never touch the database need no configuration.
        services.AddDbContext<BucklDbContext>(options =>
            BucklDbContextOptions.Configure(options, ReadConnectionString(configuration)));

        return services;
    }

    private static string ReadConnectionString(IConfiguration configuration) =>
        configuration.GetConnectionString(BucklDbContextOptions.ConnectionStringName) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"Connection string '{BucklDbContextOptions.ConnectionStringName}' is not configured. "
                + "See apps/api/README.md.");
}
