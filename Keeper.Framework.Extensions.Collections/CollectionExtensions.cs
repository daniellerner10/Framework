namespace Keeper.Framework.Extensions.Collections;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> collection2)
    {
        foreach (var item in collection2)
            collection.Add(item);
    }
}
