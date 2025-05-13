using Keeper.Framework.Collections;
using Keeper.Framework.Extensions.Collections;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Keeper.Framework.Extensions.Collections;

/// <summary>
/// Extensions for dictionary
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Implementation of AddRange for dictionaries.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary to add to.</param>
    /// <param name="other">The other dictionary to add from.</param>
    /// <param name="overrideValueOnDuplicateKeys">If false, throws exception on duplicate keys.  
    /// If true, then duplicate keys found in <paramref name="other"/>
    /// override those in the <paramref name="dictionary"/>. Defaults to false.</param>
    /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other, bool overrideValueOnDuplicateKeys = false)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(other);

        if (overrideValueOnDuplicateKeys)
        {
            foreach (var pair in other)
                dictionary[pair.Key] = pair.Value;
        }
        else
        {
            foreach (var pair in other)
                dictionary.Add(pair);
        }
    }

    /// <summary>
    /// Implementation of AddRange for dictionaries.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="dictionary">The dictionary to add to.</param>
    /// <param name="other">The other dictionary to add from.</param>
    /// <param name="overrideValueOnDuplicateKeys">If false, throws exception on duplicate keys.  
    /// If true, then duplicate keys found in <paramref name="other"/>
    /// override those in the <paramref name="dictionary"/>. Defaults to false.</param>
    /// <param name="mergeDuplicateKeysAsArray">If true, duplicate keys get merged into an array containing all unique values. Defaults to false.</param>
    /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception>
    public static void AddRange<TKey>(this IDictionary<TKey, object> dictionary, IDictionary<TKey, object> other, bool overrideValueOnDuplicateKeys = false, bool mergeDuplicateKeysAsArray = false)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(other);

        if (mergeDuplicateKeysAsArray && !overrideValueOnDuplicateKeys) throw new ArgumentException("If mergeDuplicateKeysAsArray is true, overrideValueOnDuplicateKeys must be set to true as well.");

        if (!overrideValueOnDuplicateKeys)
        {
            foreach (var pair in other)
                dictionary.Add(pair);
        }
        else if (mergeDuplicateKeysAsArray)
        {
            foreach (var pair in other)
            {
                if (dictionary.TryGetValue(pair.Key, out var currentValue))
                {
                    if (currentValue is IList<object> list)
                        list.Add(currentValue);
                    else
                        dictionary[pair.Key] = new List<object> { currentValue, pair.Value };
                }
                else
                    dictionary.Add(pair);
            }
        }
        else
        {
            foreach (var pair in other)
                dictionary[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Extracts all items from a dictionary based on a list of keys, returns them in a new
    /// dictionary and then removed them from the original dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="keys">The keys.</param>
    /// <returns>A new dictionary.</returns>
    public static IDictionary<TKey, TValue> Extract<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(keys);

        var returnValue = dictionary
            .Where(p => keys.Contains(p.Key))
            .ToDictionary(static p => p.Key, static p => p.Value);

        foreach (var pair in returnValue)
            dictionary.Remove(pair.Key);

        return returnValue;
    }

    /// <summary>
    /// Gets or add the value to the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="createFunc">The create function.</param>
    /// <returns>The value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createFunc) =>
        dictionary.GetOrAdd(key, _ => createFunc());

    /// <summary>
    /// Gets or add the value to the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="createFunc">The create function.</param>
    /// <returns>The value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> createFunc)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (!dictionary.TryGetValue(key, out var value))
            value = dictionary[key] = createFunc(key);

        return value;
    }

    public static bool TryGetValue(this System.Collections.IDictionary dictionary, object key, out object? value)
    {
        if (dictionary.Contains(key))
        {
            value = dictionary[key];
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static ref TValue GetValueRefOrNullRef<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull =>
            ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);

    public static ref TValue? GetValueRefOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out bool exists)
        where TKey : notnull =>
            ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out exists);

    public static SynchronizedDictionary<TKey, TElement> ToSynchronizedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull =>
        source.ToSynchronizedDictionary(keySelector, elementSelector, null);

    public static SynchronizedDictionary<TKey, TElement> ToSynchronizedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);

        SynchronizedDictionary<TKey, TElement> d;

        if (source is ICollection<TSource> collection)
            d = new(collection.Count, comparer);
        else
            d = new(comparer);

        foreach (var element in source)
            d.TryAdd(keySelector(element), elementSelector(element));

        return d;
    }

    public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull =>
        source.ToConcurrentDictionary(keySelector, elementSelector, null);

    public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);

        ConcurrentDictionary<TKey, TElement> d;

        if (source is ICollection<TSource> collection)
            d = new(Environment.ProcessorCount, collection.Count, comparer);
        else
            d = new(comparer);

        foreach (var element in source)
            d.TryAdd(keySelector(element), elementSelector(element));

        return d;
    }

    public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> dictionaries)
        where TKey : notnull =>
            new(dictionaries.SelectMany(d => d).Distinct());

    public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);

        OrderedDictionary<TKey, TElement> d = [];

        foreach (var element in source)
            d.TryAdd(keySelector(element), elementSelector(element));

        return d;
    }

    public static bool UpdateOrInsertIfNotEqual<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
            where TKey : notnull
            where TValue : IEquatable<TValue>
    {
        ref var currentVal = ref dictionary.GetValueRefOrAddDefault(key, out var exists)!;

        if (exists)
        {
            if (!currentVal.Equals(value))
            {
                currentVal = value;
                return true;
            }
            else
                return false;
        }
        else
        {
            currentVal = value;
            return true;
        }
    }

    public static bool UpdateOrInsertIfNotEqual<TKey1, TKey2, TValue>(
        this Dictionary<TKey1, Dictionary<TKey2, TValue>> dictionary,
        TKey1 key1,
        TKey2 key2,
        TValue value)
            where TKey1 : notnull
            where TKey2 : notnull
            where TValue : IEquatable<TValue>
    {
        ref var currentVal = ref dictionary.GetValueRefOrAddDefault(key1, out var exists)!;

        if (!exists)
            currentVal = [];

        return currentVal.UpdateOrInsertIfNotEqual(key2, value);
    }
}
