using Microsoft.EntityFrameworkCore.Design;

namespace Buckl.Infrastructure.Persistence;

/// <summary>Lets <c>dotnet ef</c> build the context without starting the API. Migrations are
/// applied with the owner role, whose connection string comes from
/// <see cref="ConnectionVariable"/> (ADR-0024). Adding a migration needs no database, so a
/// placeholder is used when the variable is not set.</summary>
public sealed class DesignTimeBucklDbContextFactory : IDesignTimeDbContextFactory<BucklDbContext>
{
    public const string ConnectionVariable = "BUCKL_MIGRATIONS_CONNECTION";

    private const string OfflinePlaceholder = "Host=localhost;Database=buckl_design_time";

    public BucklDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);

        return new BucklDbContext(BucklDbContextOptions.Build(
            string.IsNullOrWhiteSpace(connectionString) ? OfflinePlaceholder : connectionString));
    }
}
