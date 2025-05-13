namespace Keeper.Framework.Middleware.Idempotency;

/// <summary>
/// The idempotency key type.
/// </summary>
public enum IdempotencyKeyType
{
    /// <summary>
    /// String type, translates to a primary key of type [nvarchar](50).
    /// </summary>
    NVarChar50,
    /// <summary>
    /// Guid type, translates to a primary key of type [uniqueidentifier].
    /// </summary>
    Guid
}