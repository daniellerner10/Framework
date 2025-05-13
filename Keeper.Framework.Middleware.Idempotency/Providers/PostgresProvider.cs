using Npgsql;
using static Keeper.Framework.Middleware.Idempotency.DatabaseConstants;

namespace Keeper.Framework.Middleware.Idempotency;

internal class PostgresProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public PostgresProvider(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public void EnsureTableCreated(string tableName, bool clustered, IdempotencyKeyType idempotencyKeyType)
    {
        var verifySql = $"""
                CREATE SCHEMA IF NOT EXISTS {IdempotencySchemaName};
                CREATE TABLE IF NOT EXISTS {IdempotencySchemaName}."{tableName}" (
                    "Key" {GetKeyColumnType(idempotencyKeyType)} NOT NULL, 
                    "Response" text NULL,
                    "StatusCode" int NULL,
                    "Created" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT "PK_{tableName}" PRIMARY KEY ("Key")
                );                
            """;

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(verifySql, connection);

        connection.Open();
        command.ExecuteNonQuery();
        connection.Close();
    }

    public async Task<(bool conflict, string? response, int? statusCode)> GetIdempotencyResult(
        string tableName, 
        object key, 
        bool useSqlTransaction,
        CancellationToken cancellationToken)
    {
        if (useSqlTransaction)
            throw new NotSupportedException("Not supported because daniel says too slow. leave this so that Nati can say told you so.");

        await using var connection = new NpgsqlConnection(_connectionString);

        await using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = $"""
            INSERT INTO {IdempotencySchemaName}."{tableName}" ("Key") VALUES (@key)
            """;
        insertCommand.Parameters.Add(new NpgsqlParameter("@key", key));

        await connection.OpenAsync(cancellationToken);

        bool conflict = false;

        try
        {
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresDuplicateKeyConstraintViolation) 
        {
            conflict = true;
        }

        string? response = default;
        int? statusCode = default;

        if (conflict)
        {
            await using var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = $"""
            SELECT "Response", "StatusCode" FROM {IdempotencySchemaName}."{tableName}" WHERE "Key" = @key 
            """;
            selectCommand.Parameters.Add(new NpgsqlParameter("@key", key));

            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
            var success = await reader.ReadAsync(cancellationToken);

            if (success)
            {
                object value = reader["Response"];
                response = value == DBNull.Value ? null: value.ToString();

                value = reader["StatusCode"];
                statusCode = value == DBNull.Value ? null : Convert.ToInt32(value);
            }
        }

       return (conflict, response, statusCode);
    }

    public async Task UpdateIdempotencyResponse(
        string tableName, 
        object key, 
        string response, 
        int statusCode,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await using var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = $"""
            UPDATE {IdempotencySchemaName}."{tableName}" 
            SET "Response" = @response, "StatusCode" = @statusCode
            WHERE "Key" = @key
            """;
        updateCommand.Parameters.Add(new NpgsqlParameter("@response", response));
        updateCommand.Parameters.Add(new NpgsqlParameter("@statusCode", statusCode));
        updateCommand.Parameters.Add(new NpgsqlParameter("@key", key));

        await connection.OpenAsync(cancellationToken);

        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private string GetKeyColumnType(IdempotencyKeyType idempotencyKeyType) => idempotencyKeyType switch
    {
        IdempotencyKeyType.NVarChar50 => @"character varying(50) COLLATE pg_catalog.""default""",
        IdempotencyKeyType.Guid => "uuid",
        _ => throw new NotImplementedException($"Idempotency key type {idempotencyKeyType} not supported")
    };
}