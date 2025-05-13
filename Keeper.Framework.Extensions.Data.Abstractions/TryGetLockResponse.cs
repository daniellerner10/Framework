using System.Data.Common;

namespace Keeper.Framework.Extensions.Data;

public class TryGetLockResponse(DbTransaction dbTransaction, bool lockResult) 
    : IAsyncDisposable, IDisposable
{
    private DbTransaction _dbTransaction = dbTransaction;

    public bool LockAcquired { get; private set; } = lockResult;

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
                _dbTransaction.Dispose();

            disposedValue = true;
        }
    }

    ~TryGetLockResponse()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbTransaction.DisposeAsync();
        disposedValue = true;
        GC.SuppressFinalize(this);
    }

    public void Commit()
    {
        _dbTransaction.Commit();
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await _dbTransaction.CommitAsync(cancellationToken);
    }

    public void Rollback()
    {
        _dbTransaction.Rollback();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        await _dbTransaction.RollbackAsync(cancellationToken);
    }
}
