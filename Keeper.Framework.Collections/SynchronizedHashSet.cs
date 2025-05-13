using System.Collections;

namespace Keeper.Framework.Collections;

public class SynchronizedHashSet<T> : ICollection<T>
{
    public object Lock { get; } = new();

    private readonly HashSet<T> _hashSet;

    public SynchronizedHashSet()
    {
        _hashSet = [];
    }

    public SynchronizedHashSet(IEqualityComparer<T>? equalityComparer)
    {
        _hashSet = new(equalityComparer);
    }

    public SynchronizedHashSet(IEnumerable<T> collection)
    {
        _hashSet = new(collection);
    }

    public SynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T>? equalityComparer)
    {
        _hashSet = new(collection, equalityComparer);
    }

    public SynchronizedHashSet(int capacity)
    {
        _hashSet = new(capacity);
    }

    public SynchronizedHashSet(int capacity, IEqualityComparer<T>? equalityComparer)
    {
        _hashSet = new(capacity, equalityComparer);
    }

    #region Implementation of ICollection<T> 

    public bool Add(T item) => WithLock(() => _hashSet.Add(item));

    public void AddRange(IEnumerable<T> items) => WithLock(() => _hashSet.UnionWith(items));

    public void Clear() => WithLock(_hashSet.Clear);

    public bool Contains(T item) => WithLock(() => _hashSet.Contains(item));

    public bool Remove(T item) => WithLock(() => _hashSet.Remove(item));

    public int Count => WithLock(() => _hashSet.Count);

    public bool IsReadOnly => false;

    void ICollection<T>.Add(T item) => Add(item);

    public void CopyTo(T[] array, int arrayIndex) => WithLock(() => _hashSet.CopyTo(array, arrayIndex));

    public IEnumerator<T> GetEnumerator() => _hashSet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    private void WithLock(Action action)
    {
        lock (Lock)
            action();
    }

    private TResult WithLock<TResult>(Func<TResult> getValue)
    {
        lock (Lock)
            return getValue();
    }
}
