using System.Runtime.InteropServices;

namespace GarbagelessSharp;

public unsafe struct UnmanagedList<T> : IDisposable where T : unmanaged
{
    private UnmanagedArray<T> array;
    
    private uint _size;

    public int Count => (int)_size;
    public int Capacity => array.Length;

    public Span<T> AsSpan => array.AsSpan;

    public UnmanagedList() => Init(0, 16);

    public UnmanagedList(uint size, uint capacity = 16) => Init(size, capacity);

    private void Init(uint size, uint capacity)
    {
        if (_size > capacity)
            throw new ArgumentOutOfRangeException(nameof(size));

        array = new UnmanagedArray<T>(capacity);
        _size = size;
    }

    public void Add(T value)
    {
        if (!array.Allocated)
            throw new NullReferenceException();
        
        if (_size >= array.Length)
            Grow();

        array[_size] = value;
        _size++;
    }

    public void Insert(uint index, T value)
    {
        if (index > _size)
            throw new IndexOutOfRangeException();
        if (index == _size)
            Grow();
        
        array.CopyTo(index, array, index + 1, _size - index);
        _size++;

        array[index] = value;
    }

    public void RemoveAt(uint index)
    {
        if (index >= _size)
            throw new IndexOutOfRangeException();
        _size--;
        if (index < _size)
            array.CopyTo(index + 1, array, index, _size - index);
    }

    public bool Remove(T value)
    {
        int index = array.IndexOf(value);

        if (index >= 0)
        {
            RemoveAt((uint)index);
            return true;
        }

        return false;
    }

    public void RemoveLast()
    {
        _size--;
    }

    public void Clear()
    {
        _size = 0;
    }

    private void Grow()
    {
        array.Resize((uint)array.Length * 2);
    }
    
    public ref T this[int index] => ref array[index];

    public ref T this[uint index] => ref array[index];

    public Span<T> this[Range range] => array[range];

    public void Free() => Dispose();
    
    public void Dispose()
    {
        array.Dispose();
        _size = 0;
    }
}