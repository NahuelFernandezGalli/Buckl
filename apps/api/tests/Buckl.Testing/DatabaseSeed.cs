using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Npgsql;

namespace Buckl.Testing;

/// <summary>Raw reads and writes as the owner role, which bypasses Row-Level Security: for
/// arranging data and checking what is really stored, never for the behavior under test.</summary>
public static class DatabaseSeed
{
    private const string InsertGarmentSql = """
        insert into garments (id, user_id, product_id, photo_key, category, color, size,
            purchase_amount, purchase_currency, purchase_date, source, status, notes,
            created_at, updated_at, archived_at)
        values ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $14, $15)
        """;

    public static Task<UserId> InsertUserAsync(
        this PostgresDatabase database,
        CancellationToken cancellationToken) =>
        database.InsertUserAsync($"seed|{Guid.NewGuid():N}", cancellationToken);

    public static async Task<UserId> InsertUserAsync(
        this PostgresDatabase database,
        string subject,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(database);

        var id = UserId.New();
        await using var connection = await database.OpenOwnerConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            "insert into users (id, auth0_subject, created_at) values ($1, $2, $3)",
            connection);
        command.Parameters.Add(Parameter(id.Value));
        command.Parameters.Add(Parameter(subject));
        command.Parameters.Add(Parameter(TestClock.Now));
        await command.ExecuteNonQueryAsync(cancellationToken);

        return id;
    }

    public static Task<GarmentId> InsertGarmentAsync(
        this PostgresDatabase database,
        UserId owner,
        CancellationToken cancellationToken) =>
        database.InsertGarmentAsync(owner, new GarmentRow(), cancellationToken);

    public static async Task<GarmentId> InsertGarmentAsync(
        this PostgresDatabase database,
        UserId owner,
        GarmentRow row,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(row);

        await using var connection = await database.OpenOwnerConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(InsertGarmentSql, connection);
        command.Parameters.Add(Parameter(row.Id.Value));
        command.Parameters.Add(Parameter(owner.Value));
        command.Parameters.Add(Parameter(row.ProductId));
        command.Parameters.Add(Parameter(row.PhotoKey));
        command.Parameters.Add(Parameter(row.Category));
        command.Parameters.Add(Parameter(row.Color));
        command.Parameters.Add(Parameter(row.Size));
        command.Parameters.Add(Parameter(row.PurchaseAmount));
        command.Parameters.Add(Parameter(row.PurchaseCurrency));
        command.Parameters.Add(Parameter(row.PurchaseDate));
        command.Parameters.Add(Parameter(row.Source));
        command.Parameters.Add(Parameter(row.Status));
        command.Parameters.Add(Parameter(row.Notes));
        command.Parameters.Add(Parameter(row.CreatedAt));
        command.Parameters.Add(Parameter(row.ArchivedAt));
        await command.ExecuteNonQueryAsync(cancellationToken);

        return row.Id;
    }

    public static Task<bool> GarmentExistsAsync(
        this PostgresDatabase database,
        GarmentId id,
        CancellationToken cancellationToken) =>
        ExistsAsync(database, "select exists (select 1 from garments where id = $1)", id.Value, cancellationToken);

    public static Task<bool> ProductExistsAsync(
        this PostgresDatabase database,
        Guid id,
        CancellationToken cancellationToken) =>
        ExistsAsync(database, "select exists (select 1 from products where id = $1)", id, cancellationToken);

    public static async Task<string?> ReadGarmentNotesAsync(
        this PostgresDatabase database,
        GarmentId id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(database);

        await using var connection = await database.OpenOwnerConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("select notes from garments where id = $1", connection);
        command.Parameters.Add(Parameter(id.Value));

        return await command.ExecuteScalarAsync(cancellationToken) as string;
    }

    /// <summary>The local users recorded for a subject: one once it has been seen, never more.</summary>
    public static async Task<IReadOnlyList<Guid>> ReadUserIdsBySubjectAsync(
        this PostgresDatabase database,
        string subject,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(database);

        await using var connection = await database.OpenOwnerConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("select id from users where auth0_subject = $1", connection);
        command.Parameters.Add(Parameter(subject));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var ids = new List<Guid>();

        while (await reader.ReadAsync(cancellationToken))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids;
    }

    public static async Task<Guid?> ReadGarmentOwnerAsync(
        this PostgresDatabase database,
        GarmentId id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(database);

        await using var connection = await database.OpenOwnerConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("select user_id from garments where id = $1", connection);
        command.Parameters.Add(Parameter(id.Value));

        return await command.ExecuteScalarAsync(cancellationToken) as Guid?;
    }

    private static async Task<bool> ExistsAsync(
        PostgresDatabase database,
        string sql,
        Guid id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(database);

        await using var connection = await database.OpenOwnerConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(Parameter(id));

        return await command.ExecuteScalarAsync(cancellationToken) is true;
    }

    private static NpgsqlParameter Parameter(object? value) => new() { Value = value ?? DBNull.Value };
}
