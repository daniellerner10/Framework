
namespace Keeper.Framework.Extensions.Data
{
    internal interface IBinaryWriter : IAsyncDisposable
    {
        Task WriteRowAsync(CancellationToken cancellationToken, object[] values);

        ValueTask<ulong> CompleteAsync(CancellationToken cancellationToken);
    }
}
