namespace Keeper.Framework.Extensions.Data;

public enum DatabaseType
{
    /// <summary>
    /// MS Sql Server
    /// </summary>
    SqlServer,
    /// <summary>
    /// PostgreSQL
    /// </summary>
    PostgreSQL,
    /// <summary>
    /// In memory database
    /// </summary>
    InMemory,
    /// <summary>
    /// SqlLite database
    /// </summary>
    SqlLite,
    /// <summary>
    /// Can not establish the database type
    /// </summary>
    Unknown
}