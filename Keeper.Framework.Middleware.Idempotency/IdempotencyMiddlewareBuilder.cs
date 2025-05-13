using static Keeper.Framework.Middleware.Idempotency.DatabaseConstants;

namespace Keeper.Framework.Middleware.Idempotency;

internal class IdempotencyMiddlewareBuilder : IIdempotencyMiddlewareBuilder
{
    internal string DefaultConnectionString { get; set; } = default!;
    internal IdempotencyTables IdempotencyTables { get; }

    public IdempotencyMiddlewareBuilder()
    {
        IdempotencyTables = new();
    }

    public IIdempotencyMiddlewareBuilder ConfigureIdempotencyTable(string tableName = DefaultIdempotencyTableName, Action<IdempotencyTableOptions>? configureTable = null)
    {
        var options = new IdempotencyTableOptions();
        configureTable?.Invoke(options);

        IdempotencyTables[tableName] = options;

        return this;
    }

    public IIdempotencyMiddlewareBuilder WithDefaultConnectionString(string connectionString)
    {
        DefaultConnectionString = connectionString;
        return this;
    }
}