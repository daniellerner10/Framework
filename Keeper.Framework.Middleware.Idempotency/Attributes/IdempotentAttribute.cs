using static Keeper.Framework.Middleware.Idempotency.DatabaseConstants;

namespace Keeper.Framework.Middleware.Idempotency;


/// <summary>
/// Attribute used to decorate a controller or action as idempotent.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class IdempotentAttribute : Attribute
{
    /// <summary>
    /// Optional custom idempotent table name. Defaults to "Keys".
    /// </summary>
    public string IdempotencyTable { get; set; } = DefaultIdempotencyTableName;

    /// <summary>
    /// Whether or not the submission of an idempotency key is required in each request.  
    /// If true, a request without an idempotency key will throw an error.
    /// If false, a request without an idempotency key will not throw an error.
    /// If null, the policy configured in the <see cref="IdempotencyTableOptions"/>.
    /// Defaults to null.
    /// </summary>
    public bool? IsIdempotencyKeyRequired { get; set; }

    /// <summary>
    /// Whether or not the conflict will throw an exception.  
    /// If true, a request with a conflict will throw an error.
    /// Defaults to true.
    /// </summary>
    public bool ThrowExceptionOnConflict { get; set; } = true;

    /// <summary>
    /// If set to true, then the idempotency is insereted as part of a transaction
    /// that is exposted to you through the <see cref="IKeeperRequestContext"/> object.
    /// In such a case, any exception that occurs in the controller action will cause
    /// a rollback of this transaction.  In this case, it is advised to connect any
    /// database code that you run in your controller action to this transaction.  You can 
    /// use the <see cref="KeeperRequestContextExtensions.GetKeeperRequestContext(Microsoft.AspNetCore.Http.HttpContext)"/>
    /// method. Defaults to false.
    /// </summary>
    public bool UseSqlTransaction { get; set; } = false;
}