using System.Globalization;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Testing;
using Npgsql;

namespace Buckl.Infrastructure.Tests.Persistence;

/// <summary>Row-Level Security as the API's role sees it (ADR-0007). Everything here runs as
/// buckl_app; only the arrangement runs as the owner.</summary>
public class RowLevelSecurityTests
{
    private readonly PostgresDatabase _database;

    public RowLevelSecurityTests(PostgresDatabase database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Without_a_user_no_garment_is_visible()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var garment = await _database.InsertGarmentAsync(alice, Ct);
        await using var connection = await _database.OpenAppConnectionAsync(Ct);

        var visible = await VisibleGarmentsAsync(connection, transaction: null, garment);

        Assert.Empty(visible);
    }

    [Fact]
    public async Task A_user_sees_only_their_own_garments()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        var alicesGarment = await _database.InsertGarmentAsync(alice, Ct);
        var bobsGarment = await _database.InsertGarmentAsync(bob, Ct);
        await using var connection = await _database.OpenAppConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await SetUserAsync(connection, transaction, alice);

        var visible = await VisibleGarmentsAsync(connection, transaction, alicesGarment, bobsGarment);

        Assert.Equal(alicesGarment.Value, Assert.Single(visible));
    }

    [Fact]
    public async Task A_user_cannot_insert_a_garment_for_someone_else()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        await using var connection = await _database.OpenAppConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await SetUserAsync(connection, transaction, alice);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            connection,
            transaction,
            "insert into garments (id, user_id, category, color, source, status, created_at, updated_at) "
            + "values (gen_random_uuid(), $1, 'top', 'blue', 'manual', 'active', now(), now())",
            bob.Value));

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }

    [Fact]
    public async Task A_user_cannot_update_someone_elses_garment()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        var bobsGarment = await _database.InsertGarmentAsync(bob, Ct);
        await using var connection = await _database.OpenAppConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await SetUserAsync(connection, transaction, alice);

        var affected = await ExecuteAsync(
            connection,
            transaction,
            "update garments set notes = 'hijacked' where id = $1",
            bobsGarment.Value);
        await transaction.CommitAsync(Ct);

        Assert.Equal(0, affected);
        Assert.Null(await _database.ReadGarmentNotesAsync(bobsGarment, Ct));
    }

    [Fact]
    public async Task A_user_cannot_hand_a_garment_over_to_someone_else()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var bob = await _database.InsertUserAsync(Ct);
        var alicesGarment = await _database.InsertGarmentAsync(alice, Ct);
        await using var connection = await _database.OpenAppConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await SetUserAsync(connection, transaction, alice);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            connection,
            transaction,
            $"update garments set user_id = $1 where id = '{alicesGarment.Value:D}'",
            bob.Value));

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }

    [Fact]
    public async Task The_application_role_cannot_delete_garments()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var alicesGarment = await _database.InsertGarmentAsync(alice, Ct);
        await using var connection = await _database.OpenAppConnectionAsync(Ct);
        await using var transaction = await connection.BeginTransactionAsync(Ct);
        await SetUserAsync(connection, transaction, alice);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            connection,
            transaction,
            "delete from garments where id = $1",
            alicesGarment.Value));

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }

    [Fact]
    public async Task The_application_role_cannot_change_products()
    {
        await using var connection = await _database.OpenAppConnectionAsync(Ct);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            connection,
            transaction: null,
            "update products set name = 'changed' where id = $1",
            Guid.NewGuid()));

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }

    [Fact]
    public async Task A_connection_that_served_a_user_hides_every_row_afterwards_instead_of_failing()
    {
        var alice = await _database.InsertUserAsync(Ct);
        var alicesGarment = await _database.InsertGarmentAsync(alice, Ct);
        await using var connection = await _database.OpenAppConnectionAsync(Ct);
        await using (var transaction = await connection.BeginTransactionAsync(Ct))
        {
            await SetUserAsync(connection, transaction, alice);
            await transaction.CommitAsync(Ct);
        }

        // After a transaction-local set_config, the variable reads as '' (not null) on this
        // session; the policy's nullif turns that into "no user" instead of a uuid cast error.
        var visible = await VisibleGarmentsAsync(connection, transaction: null, alicesGarment);

        Assert.Empty(visible);
    }

    [Fact]
    public async Task The_application_role_can_neither_bypass_security_nor_act_as_superuser()
    {
        await using var connection = await _database.OpenOwnerConnectionAsync(Ct);
        await using var command = new NpgsqlCommand(
            "select rolbypassrls or rolsuper from pg_roles where rolname = $1",
            connection);
        command.Parameters.Add(new NpgsqlParameter { Value = PostgresDatabase.AppRole });

        var privileged = (bool?)await command.ExecuteScalarAsync(Ct);

        Assert.False(privileged);
    }

    private static async Task SetUserAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        UserId user)
    {
        await using var command = new NpgsqlCommand(
            "select set_config('app.user_id', $1, true)",
            connection,
            transaction);
        command.Parameters.Add(new NpgsqlParameter
        {
            Value = user.Value.ToString("D", CultureInfo.InvariantCulture),
        });
        await command.ExecuteNonQueryAsync(Ct);
    }

    private static async Task<List<Guid>> VisibleGarmentsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        params GarmentId[] among)
    {
        await using var command = new NpgsqlCommand(
            "select id from garments where id = any($1)",
            connection,
            transaction);
        command.Parameters.Add(new NpgsqlParameter { Value = among.Select(id => id.Value).ToArray() });

        var ids = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(Ct);
        while (await reader.ReadAsync(Ct))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids;
    }

    private static async Task<int> ExecuteAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        string sql,
        object parameter)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.Add(new NpgsqlParameter { Value = parameter });

        return await command.ExecuteNonQueryAsync(Ct);
    }
}
