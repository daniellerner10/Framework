using Keeper.Framework.Application.State;
using Keeper.Framework.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Data.Common;

namespace Keeper.Framework.Middleware;

/// <summary>
/// The keeper context for the request.
/// </summary>
public class KeeperRequestContext : ApplicationState, IKeeperRequestContext
{
    private HttpContext _context = default!;
    private DbConnection _idempotencyConnection = default!;
    private DbTransaction _idempotencyTransaction = default!;

    public bool HasConflict { get; private set; }

    /// <summary>
    /// The open connection used by the idempotency middleware. null if idempotency is not used.
    /// </summary>
    public DbConnection IdempotencyConnection
    {
        get => _idempotencyConnection;
        init => _idempotencyConnection = value;
    }

    /// <summary>
    /// The transaction used by the idempotency middleware. null if idempotency is not used.
    /// </summary>
    public DbTransaction IdempotencyTransaction
    {
        get => _idempotencyTransaction;
        init => _idempotencyTransaction = value;
    }

    HttpContext IKeeperRequestContext.HttpContext { get; set; } = default!;

    DbConnection IKeeperRequestContext.IdempotencyConnection => throw new NotImplementedException();

    DbTransaction IKeeperRequestContext.IdempotencyTransaction => throw new NotImplementedException();

    void IKeeperRequestContext.SetIdempotencyConnection(DbConnection idempotencyConnection)
    {
        _idempotencyConnection = idempotencyConnection;
    }

    void IKeeperRequestContext.SetIdempotencyTransaction(DbTransaction idempotencyTransaction)
    {
        _idempotencyTransaction = idempotencyTransaction;
    }

    void IKeeperRequestContext.SetContext(HttpContext context)
    {
        if (_context is null)
        {
            _context = context;
            TryUpdateCorrelationIdFromHeader();
        }
    }

    void IKeeperRequestContext.SetConflict()
    {
        HasConflict = true;
    }

    private void TryUpdateCorrelationIdFromHeader()
    {
        var headers = _context.Request.Headers;

        if (headers.ContainsKey(Headers.KeeperCorrelationId))
            SetCorrelationId(headers[Headers.KeeperCorrelationId].First()!);
    }
}