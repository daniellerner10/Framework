namespace Keeper.Framework.Extensions.Collections;

public static class Dictionary
{
    public static Dictionary<TKey, TValue> Empty<TKey, TValue>()
                where TKey : notnull => EmptyDictionary<TKey, TValue>.Value;
}

internal static class EmptyDictionary<TKey, TValue>
    where TKey : notnull
{
    public static readonly Dictionary<TKey, TValue> Value = [];
}
