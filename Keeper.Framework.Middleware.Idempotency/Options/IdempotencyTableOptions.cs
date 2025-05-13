namespace Keeper.Framework.Middleware.Idempotency;

public class IdempotencyTableOptions
{
    /// <summary>
    /// If this is set, then the table will use this connection string. Otherwise, it will use the
    /// default connection string that you set using the <see cref="IIdempotencyMiddlewareBuilder.WithDefaultConnectionString(string)"/> method.
    /// Defaults to null.
    /// </summary>
    public string? ConnectionString { get; set; } = null;

    /// <summary>
    /// Whether or not the primary key of the idempotency table is clustered.  Defaults to false.
    /// <b>Only set to true if your Guid is CombGuid or the string is always ordered.</b>
    /// </summary>
    public bool PrimaryKeyClustered { get; set; } = false;

    /// <summary>
    /// Whether or not the conflict will throw an exception.  
    /// If true, a request with a conflict will throw an error.
    /// Defaults to false.
    /// </summary>
    public bool ThrowExceptionOnConflict { get; set; } = false;

    /// <summary>
    /// Whether or not the submission of an idempotency key is required in each request.  
    /// If true, a request without an idempotency key will throw an error.
    /// Defaults to true.
    /// </summary>
    public bool IsIdempotencyKeyRequired { get; set; } = true;

    /// <summary>
    /// Sets the idempotency key type. Defaults to <see cref="IdempotencyKeyType.NVarChar50"/>.
    /// </summary>
    public IdempotencyKeyType KeyType { get; set; } = IdempotencyKeyType.NVarChar50;
}