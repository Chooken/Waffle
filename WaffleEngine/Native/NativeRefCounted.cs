using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WaffleEngine.Native;

public struct NativeRC<T>(T data) : IDisposable where T : unmanaged
{
    private NativePtr<Counter> _counter = new Counter();
    private NativePtr<T> _data = data;

    public bool IsRefAlive => !_data.IsNull;
    
    public ref T Value => ref _data.Value;

    public bool TryGetStrongRef([NotNullWhen(true)]out NativeRC<T>? value)
    {
        value = null;
        
        if (_data.IsNull || _counter.IsNull)
            return false;
        
        Interlocked.Increment(ref _counter.Value.StrongReferences);
        value = this;
        return true;
    }

    public bool TryGetWeakRef([NotNullWhen(true)]out NativeWeakPtr<T>? value)
    {
        value = null;
        
        if (_data.IsNull || _counter.IsNull)
            return false;
        
        Interlocked.Increment(ref _counter.Value.WeakReferences);
        value = new NativeWeakPtr<T>(_data, _counter);
        return true;
    }

    public void Dispose()
    {
        if (!IsRefAlive)
            return;
        
        Interlocked.Decrement(ref _counter.Value.StrongReferences);
        
        if (_counter.Value.StrongReferences == 0)
            _data.Dispose();
        
        if (_counter.Value.WeakReferences == 0)
            _counter.Dispose();

        _data = NativePtr<T>.Null;
        _counter = NativePtr<Counter>.Null;
    }
}

public struct NativeWeakPtr<T>(NativePtr<T> data, NativePtr<Counter> counter) : IDisposable where T : unmanaged
{
    private NativePtr<Counter> _counter = counter;
    private NativePtr<T> _data = data;

    public bool IsRefAlive => _counter.Value.StrongReferences != 0;

    public ref T Value => ref _data.Value;

    public bool TryGetValue([NotNullWhen(true)] out T value)
    {
        if (!IsRefAlive)
        {
            value = default;
            return false;
        }

        value = _data.Value;
        return true;
    }

    public bool TrySetValue(T value)
    {
        if (!IsRefAlive)
        {
            return false;
        }

        _data.Value = value;
        return true;
    }
    
    public void Dispose()
    {
        if (_counter.IsNull)
            return;

        Interlocked.Decrement(ref _counter.Value.WeakReferences);

        if (_counter.Value.WeakReferences == 0)
        {
            _counter.Dispose();
        }

        _data = NativePtr<T>.Null;
        _counter = NativePtr<Counter>.Null;
    }
}

public struct Counter()
{
    public uint StrongReferences = 1;
    public uint WeakReferences = 0;
}