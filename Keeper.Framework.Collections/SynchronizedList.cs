using System.Collections;

namespace Keeper.Framework.Collections;

public class SynchronizedList<T> : IList<T>
{
    public object Lock { get; } = new();

    private readonly List<T> _list = [];

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => WithLock(() => _list[index]);
        set => WithLock(() => _list[index] = value);
    }

    #region Implementation of IList<T> 

    public int IndexOf(T item) => WithLock(() => _list.IndexOf(item));

    public void Insert(int index, T item) => WithLock(() => _list.Insert(index, item));

    public void RemoveAt(int index) => WithLock(() => _list.RemoveAt(index));

    public void Add(T item) => WithLock(() => _list.Add(item));

    public void Clear() => WithLock(_list.Clear);

    public bool Contains(T item) => WithLock(() => _list.Contains(item));

    public bool Remove(T item) => WithLock(() => _list.Remove(item));

    public void CopyTo(T[] array, int arrayIndex) => WithLock(() => _list.CopyTo(array, arrayIndex));

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

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
