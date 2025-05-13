using System.Runtime.CompilerServices;

namespace Keeper.Framework.Extensions.Collections;

public static class AsyncEnumerableExtensions
{
    public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> list, CancellationToken cancellationToken = default)
    {
        var builder = new ArrayBuilder<T>(0);
        await foreach (var item in list.WithCancellation(cancellationToken))
            builder.Add(item);

        return builder.ToArray();
    }

    public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> list, CancellationToken cancellationToken = default)
    {
        await using var enumerator = list.GetAsyncEnumerator(cancellationToken);
        if (await enumerator.MoveNextAsync())
            return enumerator.Current;
        else
            throw new InvalidOperationException("The source sequence is empty.");
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> list, CancellationToken cancellationToken = default)
    {
        await using var enumerator = list.GetAsyncEnumerator(cancellationToken);
        if (await enumerator.MoveNextAsync())
            return enumerator.Current;
        else
            return default;
    }

    public static async IAsyncEnumerable<List<T>> Chunk<T>(this IAsyncEnumerable<T> list, int size, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var enumerator = list.GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            var chunk = await GetNextChunk(enumerator, size);

            if (chunk.Count > 0)
                yield return chunk;
            else
                break;
        }
    }

    private static async Task<List<T>> GetNextChunk<T>(IAsyncEnumerator<T> enumerator, int size)
    {
        var list = new List<T>();

        while (list.Count <= size && await enumerator.MoveNextAsync())
            list.Add(enumerator.Current);

        return list;
    }
}