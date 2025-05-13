using Keeper.Framework.Extensions.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Keeper.Framework.Middleware.Idempotency;

/// <summary>
/// Extension methods for the idempotency middleware.
/// </summary>
public static class IdempotencyExtensions
{
    /// <summary>
    /// This needs to be put after app.UserRouting() and before app.UseEndpoints();
    /// </summary>
    /// <param name="app">The app builder</param>
    /// <param name="configure">A callback for configuring the middleware.</param>
    /// <returns>The app builder.</returns>
    public static IApplicationBuilder UseKeeperIdempotencyMiddleware(this IApplicationBuilder app, Action<IIdempotencyMiddlewareBuilder> configure)
    {
        IdempotencyMiddleware.StepIntoForDebugger();

        var idempotencyMiddlewareBuilder = new IdempotencyMiddlewareBuilder();
        configure(idempotencyMiddlewareBuilder);

        if (string.IsNullOrWhiteSpace(idempotencyMiddlewareBuilder.DefaultConnectionString) &&
            (idempotencyMiddlewareBuilder.IdempotencyTables.IsEmpty ||
             idempotencyMiddlewareBuilder.IdempotencyTables.Any(static x => string.IsNullOrWhiteSpace(x.Value.ConnectionString))))
            throw new ArgumentException("You must provide a valid default connection string or every table must have a valid connection string in order to use the idempotency middleware.");

        if (idempotencyMiddlewareBuilder.IdempotencyTables.IsEmpty)
            idempotencyMiddlewareBuilder.ConfigureIdempotencyTable();

        VerifySchemaAndTables(idempotencyMiddlewareBuilder);

        IdempotencyMiddleware.DefaultConnectionString = idempotencyMiddlewareBuilder.DefaultConnectionString;
        IdempotencyMiddleware.IdempotencyTables = idempotencyMiddlewareBuilder.IdempotencyTables;

        app.UseMiddleware<IdempotencyMiddleware>();

        return app;
    }

    /// <summary>
    /// Used to register a custom idempotency key selector.
    /// </summary>
    /// <typeparam name="TKeySelector">Your custom class which implements the <see cref="IIdempotencyKeySelector"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection UseCustomIdempotencyKeySelector<TKeySelector>(this IServiceCollection services)
        where TKeySelector : class, IIdempotencyKeySelector
    {
        return services.AddSingleton<IIdempotencyKeySelector, TKeySelector>();
    }

    private static void VerifySchemaAndTables(IdempotencyMiddlewareBuilder idempotencyMiddlewareBuilder)
    {
        foreach (var (tableName, options) in idempotencyMiddlewareBuilder.IdempotencyTables)
        {
            var connectionString = options.ConnectionString ?? idempotencyMiddlewareBuilder.DefaultConnectionString;
            var databaseProvider = connectionString.GetDatabaseProvider();

            databaseProvider.EnsureTableCreated(
                tableName,
                options.PrimaryKeyClustered,
                options.KeyType);
        }
    }

    internal static IDatabaseProvider GetDatabaseProvider(this string connectionString)
    {
        var databaseType = connectionString.GetDatabaseTypeFromConnectionString();

        return databaseType switch
        {
            DatabaseType.PostgreSQL => new PostgresProvider(connectionString),
            DatabaseType.Unknown => throw new NotSupportedException($"Can not detect database type for connection string: {connectionString}"),
            _ => throw new NotSupportedException($"Idempotency does not support database type {databaseType}")
        };
    }
}