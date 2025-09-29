using System.Collections;
using System.Runtime.InteropServices;

namespace WaffleEngine.Native;

public unsafe struct NativeArray<T> : IDisposable, IEnumerable<T> where T : unmanaged
{
    private T* _pointer;
    private uint _size;

    public int Length => (int)_size;

    public Span<T> AsSpan => new Span<T>(_pointer, (int)_size);
    
    public bool Allocated => _pointer != (T*)0x0;

    public NativeArray(uint size)
    {
        _size = size;
        _pointer = (T*)NativeMemory.Alloc(_size, (uint)sizeof(T));
    }

    public NativeArray(NativePtr<T> nativePtr, uint size)
    {
        _size = size;
        _pointer = nativePtr;
    }

    public void Resize(uint size)
    {
        _size = size;
        _pointer = (T*)NativeMemory.Realloc(_pointer, size * (uint)sizeof(T));
    }

    public int IndexOf(T value)
    {
        for (int i = 0; i < _size; i++)
        {
            if (_pointer[i].Equals(value))
                return i;
        }

        return -1;
    }

    public void CopyTo(uint index, NativeArray<T> destination, uint destinationIndex, uint length)
    {
        if (length == 0)
            return;
        
        if (_size < index + length || destination._size < index + length)
            throw new IndexOutOfRangeException();
        
        NativeMemory.Copy(
            _pointer + index, 
            destination._pointer + destinationIndex, 
            length * (uint)sizeof(T));
    }
    
    public ref T this[uint index]
    {
        get
        {
            if (index > _size)
                throw new IndexOutOfRangeException();
                    
            return ref _pointer[index];
        }
    }
    
    public ref T this[int index]
    {
        get
        {
            if (index > _size)
                throw new IndexOutOfRangeException();
            
            if (index < 0)
                throw new IndexOutOfRangeException();
                    
            return ref _pointer[index];
        }
    }

    public Span<T> this[Range range]
    {
        get
        {
            if (range.Start.Value > _size || range.End.Value > _size)
                throw new IndexOutOfRangeException();
            
            if (range.Start.Value < 0 || range.End.Value < 0)
                throw new IndexOutOfRangeException();
            
            return new Span<T>(_pointer + range.Start.Value, range.End.Value - range.Start.Value);
        }
    }

    public ref T Get(int index)
    {
        return ref _pointer[index];
    }

    public void Free() => Dispose();

    public void Dispose()
    {
        if (_pointer == (T*)0x0)
            throw new NullReferenceException();
        
        NativeMemory.Free(_pointer);
        _pointer = (T*)0x0;
        _size = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _size; i++)
        {
            yield return Get(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}