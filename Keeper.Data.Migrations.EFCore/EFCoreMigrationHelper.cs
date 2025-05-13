using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Keeper.Data.Migrations.EFCore;

public static class EFCoreMigrationHelper
{
    public const string InMemoryDatabaseProviderName = "Microsoft.EntityFrameworkCore.InMemory";

    public static void Migrate<TContext>(this IHost host, string? targetMigration = default)
        where TContext : DbContext =>
            host.Services.Migrate<TContext>(targetMigration);

    public static Task MigrateAsync<TContext>(this IHost host, string? targetMigration = default, CancellationToken cancellationToken = default)
        where TContext : DbContext =>
            host.Services.MigrateAsync<TContext>(targetMigration, cancellationToken);

    public static void Migrate<TContext>(this IServiceCollection serviceCollection, string? targetMigration = default)
        where TContext : DbContext =>
            serviceCollection.BuildServiceProvider().Migrate<TContext>(targetMigration);

    public static Task MigrateAsync<TContext>(this IServiceCollection serviceCollection, string? targetMigration = default, CancellationToken cancellationToken = default)
        where TContext : DbContext =>
            serviceCollection.BuildServiceProvider().MigrateAsync<TContext>(targetMigration, cancellationToken);

    public static void Migrate<TContext>(this IServiceProvider provider, string? targetMigration = default)
        where TContext : DbContext
    {
        var context = provider.GetService<TContext>() ??
                      provider.GetRequiredService<IDbContextFactory<TContext>>().CreateDbContext();

        context.Migrate(targetMigration);
    }

    public static async Task MigrateAsync<TContext>(this IServiceProvider provider, string? targetMigration = default, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        using var context = provider.GetService<TContext>() ??
                      provider.GetRequiredService<IDbContextFactory<TContext>>().CreateDbContext();

        await context.MigrateAsync(targetMigration, cancellationToken);
    }

    public static void Migrate(this DbContext context, string? targetMigration = default)
    {
        if (context.Database.ProviderName != InMemoryDatabaseProviderName)
        {
            CreateDatabaseIfNotExists(context.Database.GetConnectionString()!);

            if (!string.IsNullOrWhiteSpace(targetMigration))
            {
                var migrator = context.Database.GetService<IMigrator>();
                migrator.Migrate(targetMigration);
            }
            else
                context.Database.Migrate();
        }
    }

    public static async Task MigrateAsync(this DbContext context, string? targetMigration = default, CancellationToken cancellationToken = default)
    {
        if (context.Database.ProviderName != InMemoryDatabaseProviderName)
        {
            await CreateDatabaseIfNotExistsAsync(context.Database.GetConnectionString()!);

            if (!string.IsNullOrWhiteSpace(targetMigration))
            {
                var migrator = context.Database.GetService<IMigrator>();
                await migrator.MigrateAsync(targetMigration, cancellationToken);
            }
            else
                await context.Database.MigrateAsync(cancellationToken);
        }
    }

    private static void CreateDatabaseIfNotExists(string postgreSqlConnectionString)
    {
        var connBuilder = new NpgsqlConnectionStringBuilder
        {
            ConnectionString = postgreSqlConnectionString
        };

        var dbName = connBuilder.Database!;

        var masterConnection = postgreSqlConnectionString.Replace(dbName, "postgres");

        using var connection = new NpgsqlConnection(masterConnection);
        connection.Open();

        using var checkIfExistsCommand = new NpgsqlCommand($"SELECT 1 FROM pg_catalog.pg_database WHERE datname = '{dbName}'", connection);
        var result = checkIfExistsCommand.ExecuteScalar();

        if (result is null)
        {
            try
            {
                using var command = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", connection);
                command.ExecuteNonQuery();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateDatabase)
            {
            }
        }

        postgreSqlConnectionString = masterConnection.Replace("postgres", dbName);
    }

    private static async Task CreateDatabaseIfNotExistsAsync(string postgreSqlConnectionString)
    {
        var connBuilder = new NpgsqlConnectionStringBuilder
        {
            ConnectionString = postgreSqlConnectionString
        };

        var dbName = connBuilder.Database!;

        var masterConnection = postgreSqlConnectionString.Replace(dbName, "postgres");

        using var connection = new NpgsqlConnection(masterConnection);
        await connection.OpenAsync();

        using var checkIfExistsCommand = new NpgsqlCommand($"SELECT 1 FROM pg_catalog.pg_database WHERE datname = '{dbName}'", connection);
        var result = await checkIfExistsCommand.ExecuteScalarAsync();

        if (result is null)
        {
            try
            {
                using var command = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateDatabase)
            {
            }
        }

        postgreSqlConnectionString = masterConnection.Replace("postgres", dbName);
    }
}