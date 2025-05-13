using System.Diagnostics;

namespace Keeper.Framework.Extensions.Collections;

public class ArrayBuilder<T>
{
    private const uint DefaultCapacity = 4;
    private const int MaxCoreClrArrayLength = 0x7fefffff; // For byte arrays the limit is slightly larger

    private T[]? _array; // Starts out null, initialized on first Add.
    private uint _count; // Number of items into _array we're using.

    /// <summary>
    /// Initializes the <see cref="ArrayBuilder{T}"/> with a specified capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the array to allocate.</param>
    public ArrayBuilder(uint capacity)
    {
        if (capacity > 0)
            _array = new T[capacity];
    }

    public ArrayBuilder() : this(0)
    {
    }

    public ArrayBuilder(int capacity)
    {
        if (capacity > 0)
            _array = new T[capacity];
        else if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be a non negative value.");
    }

    /// <summary>
    /// Gets the number of items this instance can store without re-allocating,
    /// or 0 if the backing array is <c>null</c>.
    /// </summary>
    public uint Capacity => (uint)(_array?.Length ?? 0);

    /// <summary>Gets the current underlying array.</summary>
    public T[]? Buffer => _array;

    /// <summary>
    /// Gets the number of items in the array currently in use.
    /// </summary>
    public uint Count => _count;

    /// <summary>
    /// Gets or sets the item at a certain index in the array.
    /// </summary>
    /// <param name="index">The index into the array.</param>
    public T this[uint index]
    {
        get
        {
            if (index < _count)
                throw new IndexOutOfRangeException($"Index {index} is out of range.");

            return _array![index];
        }
    }

    public void Clear()
    {
        _count = 0;
    }

    /// <summary>
    /// Adds an item to the backing array, resizing it if necessary.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        if (_count == Capacity)
            EnsureCapacity(_count + 1);

        UncheckedAdd(item);
    }

    public void AddArray(T[] arr)
    {
        var newCapacity = _count + (uint)arr.Length;
        if (newCapacity > Capacity)
            EnsureCapacity(newCapacity);

        UncheckedAddArray(arr);
    }


    /// <summary>
    /// Gets the first item in this builder.
    /// </summary>
    public T First()
    {
        if (_count == 0)
            throw new IndexOutOfRangeException("There are no elements");

        return _array![0];
    }

    /// <summary>
    /// Gets the last item in this builder.
    /// </summary>
    public T Last()
    {
        if (_count == 0)
            throw new IndexOutOfRangeException("There are no elements");

        return _array![_count - 1];
    }

    /// <summary>
    /// Creates an array from the contents of this builder.
    /// </summary>
    /// <remarks>
    /// Do not call this method twice on the same builder.
    /// </remarks>
    public T[] ToArray()
    {
        if (_count == 0)
            return [];

        T[] result = _array!;
        if (_count < result.Length)
        {
            // Avoid a bit of overhead (method call, some branches, extra codegen)
            // which would be incurred by using Array.Resize
            result = new T[_count];
            Array.Copy(_array!, result, _count);
        }

        return result;
    }

    /// <summary>
    /// Adds an item to the backing array, without checking if there is room.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <remarks>
    /// Use this method if you know there is enough space in the <see cref="ArrayBuilder{T}"/>
    /// for another item, and you are writing performance-sensitive code.
    /// </remarks>
    public void UncheckedAdd(T item)
    {
        Debug.Assert(_count < Capacity);

        _array![_count++] = item;
    }

    public void UncheckedAddArray(T[] arr)
    {
        Debug.Assert(_count < Capacity);

        Array.Copy(arr, 0, _array!, _count, arr.Length);

        _count += (uint)arr.Length;
    }

    private void EnsureCapacity(uint minimum)
    {
        uint capacity = Capacity;
        uint nextCapacity = capacity == 0 ? DefaultCapacity : 2 * capacity;

        if (nextCapacity > MaxCoreClrArrayLength)
            nextCapacity = Math.Max(capacity + 1, MaxCoreClrArrayLength);

        nextCapacity = Math.Max(nextCapacity, minimum);

        var next = new T[nextCapacity];
        if (_count > 0)
            Array.Copy(_array!, next, _count);

        _array = next;
    }
}
