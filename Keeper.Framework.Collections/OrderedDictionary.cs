using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Keeper.Framework.Collections;

public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly object _syncRoot = new();
    private readonly List<KeyValuePair<TKey, TValue>> _list;
    private readonly Dictionary<TKey, TValue> _dictionary;

    public OrderedDictionary()
    {
        _list = [];
        _dictionary = [];
    }

    public OrderedDictionary(int capacity)
    {
        _list = new(capacity);
        _dictionary = new(capacity);
    }

    public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
        _list = new(collection);
        _dictionary = new(collection);
    }

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => Set(key, value);
    }

    public KeyValuePair<TKey, TValue> this[int index]
    {
        get => _list[index];
        set => Insert(index, value.Key, value.Value);
    }

    public ICollection<TKey> Keys => _list.Select(x => x.Key).ToList();

    public ICollection<TValue> Values => _list.Select(x => x.Value).ToList();

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public bool IsSynchronized => false;

    public object SyncRoot => _syncRoot;

    public void Set(TKey key, TValue value)
    {
        if (_dictionary.ContainsKey(key))
        {
            _dictionary[key] = value;
            var index = IndexOf(key);
            _list[index] = new KeyValuePair<TKey, TValue>(key, value);
        }
        else
            Add(key, value);
    }

    public void Add(TKey key, TValue value)
    {
        var pair = new KeyValuePair<TKey, TValue>(key, value);
        _list.Add(pair);
        _dictionary.Add(key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _list.Add(item);
        _dictionary.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _list.Clear();
        _dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        _dictionary.TryGetValue(item.Key, out var value) && (item.Value?.Equals(value) ?? value is null);

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        for (var i = 0; i < _list.Count; i++)
            array[i + arrayIndex] = _list[i];
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();

    public void Insert(int index, TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        _list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
    }

    public bool Remove(TKey key)
    {
        var found = _dictionary.Remove(key);
        if (found)
        {
            var index = IndexOf(key);
            _list.RemoveAt(index);
        }
        return found;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var index = _list.IndexOf(item);

        if (index >= 0)
        {
            _list.RemoveAt(index);
            _dictionary.Remove(item.Key);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        var key = _list[index].Key;
        _list.RemoveAt(index);
        _dictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        _dictionary.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    public int IndexOf(TKey key)
    {
        return _list.FindIndex(x => x.Key?.Equals(key) ?? key is null);
    }

    public ref TValue? GetValueRefOrAddDefault(TKey key, out bool exists)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out exists);

        if (!exists)
        {
            _list.Add(new KeyValuePair<TKey, TValue>(key, value!));
        }

        return ref value;
    }

    public ref TValue? GetValueRefOrNullRef(TKey key)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(_dictionary, key);

        if (value is null)
            _list.Add(new KeyValuePair<TKey, TValue>(key, value));

        return ref value;
    }
}