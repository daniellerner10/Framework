namespace Keeper.Framework.Extensions.Collections;

public static class HashSet
{
    public static HashSet<T> Empty<T>() => EmptyHashSet<T>.Value;

    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
    {
        foreach (var item in collection)
            hashSet.Add(item);
    }

    public static void Remove<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
    {
        foreach (var item in collection)
            hashSet.Remove(item);
    }
}

internal static class EmptyHashSet<T>
{
    public readonly static HashSet<T> Value = [];
}
