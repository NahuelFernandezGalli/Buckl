using Buckl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Buckl.Testing;

/// <summary>One Postgres 17 container per test assembly, set up the way Neon is: the owner role
/// (here the container's superuser) applies the migrations, and <see cref="AppRole"/> is the
/// role the API uses at runtime, without BYPASSRLS. The owner bypasses Row-Level Security, so
/// tests arrange data as the owner and act as the application role. Tests isolate themselves by
/// using fresh ids, never by cleaning tables.</summary>
public sealed class PostgresDatabase : IAsyncLifetime
{
    public const string AppRole = "buckl_app";

    private const string AppPassword = "buckl_app_test_password";

    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string OwnerConnectionString => _container.GetConnectionString();

    public string AppConnectionString =>
        new NpgsqlConnectionStringBuilder(OwnerConnectionString)
        {
            Username = AppRole,
            Password = AppPassword,
        }.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await _container.StartAsync(cancellationToken);

        // Same statement the Neon setup runs from the console (apps/api/README.md).
        await ExecuteAsOwnerAsync(
            $"create role {AppRole} login password '{AppPassword}' nobypassrls",
            cancellationToken);

        await using var context = CreateOwnerContext();
        await context.Database.MigrateAsync(cancellationToken);
    }

    public BucklDbContext CreateOwnerContext() => new(BucklDbContextOptions.Build(OwnerConnectionString));

    public BucklDbContext CreateAppContext() => new(BucklDbContextOptions.Build(AppConnectionString));

    public Task<NpgsqlConnection> OpenOwnerConnectionAsync(CancellationToken cancellationToken) =>
        OpenAsync(OwnerConnectionString, cancellationToken);

    public Task<NpgsqlConnection> OpenAppConnectionAsync(CancellationToken cancellationToken) =>
        OpenAsync(AppConnectionString, cancellationToken);

    public async Task ExecuteAsOwnerAsync(string sql, CancellationToken cancellationToken)
    {
        await using var connection = await OpenOwnerConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();

    private static async Task<NpgsqlConnection> OpenAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
