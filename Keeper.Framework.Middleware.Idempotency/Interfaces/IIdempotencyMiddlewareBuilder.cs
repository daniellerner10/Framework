using static Keeper.Framework.Middleware.Idempotency.DatabaseConstants;

namespace Keeper.Framework.Middleware.Idempotency;

/// <summary>
/// The idempotency middleware builder.
/// </summary>
public interface IIdempotencyMiddlewareBuilder
{
    /// <summary>
    /// Sets the connection string to use for idempotency.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The builder.</returns>
    IIdempotencyMiddlewareBuilder WithDefaultConnectionString(string connectionString);

    /// <summary>
    /// Configures an idempotency table.  If this method is not called, a default table is still created.
    /// </summary>
    /// <param name="tableName">The name of the table.  Table will be created in the [idempotency] schema.  Defaults to [Keys].</param>
    /// <param name="configureTable">A method used to configure the idempotency table.  Defaults to null.</param>
    /// <returns></returns>
    IIdempotencyMiddlewareBuilder ConfigureIdempotencyTable(string tableName = DefaultIdempotencyTableName, Action<IdempotencyTableOptions>? configureTable = null);
}