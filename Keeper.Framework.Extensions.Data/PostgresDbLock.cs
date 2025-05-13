using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Security;

namespace Keeper.Framework.Extensions.Data;

public static class PostgresDbLock
{
    public static Task<TryGetLockResponse> TryGetAppLockTransaction(this DbContext context, string resourceName, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        return connection.TryGetAppLockTransaction(resourceName, cancellationToken);
    }

    public static async Task<TryGetLockResponse> TryGetAppLockTransaction(this DbConnection connection, string resourceName, CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();

        if (connection.State != ConnectionState.Open)
            connection.Open();

        //resourceName shouold be int. The default GetHashCode isn't deterministic so it requires a custom implementation.
        var hashcode = GetHashCode(resourceName);
        command.CommandText =$"SELECT pg_try_advisory_xact_lock({hashcode}); -- {resourceName}";

        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        command.Transaction = transaction;

        var commandResult = await command.ExecuteScalarAsync(cancellationToken);

        return new(transaction, commandResult is bool cmdResBool && cmdResBool);
    }

    public static Task<DbTransaction> GetAppLockTransaction(this DbContext context, string resourceName, string actionName, TimeSpan lockTimeout, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        return connection.GetAppLockTransaction(resourceName, actionName, lockTimeout, cancellationToken);
    }

    public static async Task<DbTransaction> GetAppLockTransaction(this DbConnection connection, string resourceName, string actionName, TimeSpan lockTimeout, CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();

        if (connection.State != ConnectionState.Open)
            connection.Open();

        //resourceName shouold be int. The default GetHashCode isn't deterministic so it requires a custom implementation.
        var hashcode = GetHashCode(resourceName);
        command.CommandText =
          $"""
          DO $$
          BEGIN
            SET LOCAL lock_timeout TO '{lockTimeout.TotalMilliseconds}ms';
        
            PERFORM pg_advisory_xact_lock({hashcode}); -- {resourceName}
     
            SET LOCAL lock_timeout TO '0ms';
          END
          $$
          """;

        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        command.Transaction = transaction;
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.LockNotAvailable)
        {
            if (ex.Where?.Contains($"SELECT pg_advisory_xact_lock({hashcode})") is true)
                throw new ConcurrentConflictException(resourceName, actionName, ex);

            throw;
        }

        return transaction;
    }

    private static async Task ReleaseAllTransactionAppLocks(DbConnection connection, CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();

        if (connection.State != ConnectionState.Open)
            connection.Open();

        command.CommandText = "SELECT pg_advisory_unlock_all();";

        using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        command.Transaction = transaction;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    [SecuritySafeCritical]
    private static int GetHashCode(string str)
    {
        var strToLower = str.ToLowerInvariant();

        unsafe
        {
            fixed (char* src = strToLower)
            {
                int hash1 = 5381;
                int hash2 = hash1;

                int c;
                char* s = src;
                while ((c = s[0]) != 0)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ c;
                    c = s[1];
                    if (c == 0)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ c;
                    s += 2;
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
