using System.Collections;

namespace Keeper.Framework.Collections;

/// <summary>
/// A synchronized dictionary that employs a lock.  Faster than <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public class SynchronizedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly object _lock = new();

    /// <summary>
    /// Ctor
    /// </summary>
    public SynchronizedDictionary()
    {
        _dictionary = [];
    }

    /// <summary>
    /// Ctor with comparer
    /// </summary>
    /// <param name="comparer"></param>
    public SynchronizedDictionary(IEqualityComparer<TKey>? comparer)
    {
        _dictionary = new(comparer);
    }

    public SynchronizedDictionary(int capacity, IEqualityComparer<TKey>? comparer)
    {
        _dictionary = new(capacity, comparer);
    }

    /// <inheritdoc/>
    public TValue this[TKey key]
    {
        get => WithLock(() => _dictionary[key]);
        set => WithLock(() => _dictionary[key] = value);
    }

    /// <inheritdoc/>
    public ICollection<TKey> Keys => WithLock(() => _dictionary.Keys);

    /// <inheritdoc/>
    public ICollection<TValue> Values => WithLock(() => _dictionary.Values);

    /// <inheritdoc/>
    public int Count => WithLock(() => _dictionary.Count);

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public void Add(TKey key, TValue value) => WithLock(() => _dictionary.Add(key, value));

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <inheritdoc/>
    public void Clear() => WithLock(_dictionary.Clear);

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        WithLock(() => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item));

    /// <inheritdoc/>
    public bool ContainsKey(TKey key) => WithLock(() => _dictionary.ContainsKey(key));

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
        WithLock(() =>
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array!, arrayIndex));

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        WithLock(_dictionary.GetEnumerator);

    /// <summary>
    /// Gets or adds a key value pair.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="getValue">Factory for the value.</param>
    /// <returns>The value.</returns>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> getValue) => WithLock(() =>
    {
        if (!_dictionary.TryGetValue(key, out var value))
        {
            value = getValue(key);
            _dictionary.Add(key, value);
        }

        return value;
    });

    /// <inheritdoc/>
    public bool Remove(TKey key) => WithLock(() => _dictionary.Remove(key));

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_lock)
            return _dictionary.TryGetValue(key, out value!);
    }

    public bool TryAdd(TKey key, TValue value) =>
        WithLock(() => _dictionary.TryAdd(key, value));

    public bool TryUpdate(TKey key, TValue value) =>
        WithLock(() =>
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = value;
                return true;
            }
            else
                return false;
        });

    public bool TryUpdate(TKey key, Func<TValue, TValue> updateFunc) =>
        WithLock(() =>
        {
            if (_dictionary.TryGetValue(key, out TValue? value))
            {
                _dictionary[key] = updateFunc(value);
                return true;
            }
            else
                return false;
        });

    public TValue Update(TKey key, Func<TValue, TValue> updateFunc) =>
        WithLock(() => _dictionary[key] = updateFunc(_dictionary[key]));

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void WithLock(Action action)
    {
        lock (_lock)
            action();
    }

    private TResult WithLock<TResult>(Func<TResult> getValue)
    {
        lock (_lock)
            return getValue();
    }
}

public static class SynchronizedDictionary
{
    public static SynchronizedDictionary<TKey, TValue> Empty<TKey, TValue>()
                where TKey : notnull => EmptySynchronizedDictionary<TKey, TValue>.Value;
}

internal static class EmptySynchronizedDictionary<TKey, TValue>
    where TKey : notnull
{
    public static readonly SynchronizedDictionary<TKey, TValue> Value = [];
}
