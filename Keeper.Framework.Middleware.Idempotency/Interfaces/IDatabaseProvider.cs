using System.Data.Common;

namespace Keeper.Framework.Middleware.Idempotency;

internal interface IDatabaseProvider
{
    void EnsureTableCreated(string tableName, bool clustered, IdempotencyKeyType idempotencyKeyType);

    Task<(bool conflict, string? response, int? statusCode)> GetIdempotencyResult(string tableName, object key, bool useSqlTransaction, CancellationToken cancellationToken);

    Task UpdateIdempotencyResponse(string tableName, object key, string response, int statusCode, CancellationToken cancellationToken);
}