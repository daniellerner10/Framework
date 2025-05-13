using Keeper.Framework.Application.State;
using Microsoft.AspNetCore.Http;
using System.Data.Common;

namespace Keeper.Framework.Middleware;

public interface IKeeperRequestContext : IApplicationState
{
    /// <summary>
    /// The open connection used by the idempotency middleware. null if idempotency is not used.
    /// </summary>
    DbConnection IdempotencyConnection { get; }

    /// <summary>
    /// The transaction used by the idempotency middleware. null if idempotency is not used.
    /// </summary>
    DbTransaction IdempotencyTransaction { get; }

    internal HttpContext HttpContext { get; set; }

    internal void SetIdempotencyConnection(DbConnection idempotencyConnection);
    internal void SetIdempotencyTransaction(DbTransaction idempotencyTransaction);
    internal void SetContext(HttpContext context);
    internal void SetConflict();
}