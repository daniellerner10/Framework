namespace Keeper.Framework.Extensions.Data;

/// <summary>
/// Extensions for connection strings
/// </summary>
public static class ConnectionStringExtensions
{
    /// <summary>
    /// Gets the database type from a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The database type.</returns>
    public static DatabaseType GetDatabaseTypeFromConnectionString(this string connectionString) => DatabaseType.PostgreSQL;
}