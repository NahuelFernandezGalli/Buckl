using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Persistence;

/// <summary>The one place that decides how the context talks to Postgres, shared by dependency
/// injection, the design-time factory and the tests.</summary>
public static class BucklDbContextOptions
{
    /// <summary>Configuration key <c>ConnectionStrings:Buckl</c>: the application role, never the
    /// owner.</summary>
    public const string ConnectionStringName = "Buckl";

    public static void Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        builder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
    }

    public static DbContextOptions<BucklDbContext> Build(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<BucklDbContext>();
        Configure(builder, connectionString);

        return builder.Options;
    }
}
