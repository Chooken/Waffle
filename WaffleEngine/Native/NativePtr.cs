using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WaffleEngine.Native;

public unsafe struct NativePtr<T> : IDisposable where T : unmanaged
{
    private T* _pointer;
    
    public NativePtr(T value)
    {
        _pointer = (T*) NativeMemory.Alloc((UIntPtr)sizeof(T));
        *_pointer = value;
    }
    
    public NativePtr(IntPtr value)
    {
        _pointer = (T*)value;
    }
    
    public void Free() => Dispose();

    public void Dispose()
    {
        if (_pointer == (T*)0x0)
            throw new NullReferenceException();
        
        NativeMemory.Free(_pointer);
        _pointer = (T*)0x0;
    }

    public void ReAlloc(uint size)
    {
        if (Null)
            return;

        _pointer = (T*)NativeMemory.Realloc(_pointer, size * (uint)sizeof(T));
    }

    public ref T Value => ref *_pointer;

    public bool Null => _pointer == null;

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = null;
        
        if (_pointer == (T*)0x0)
            return false;

        value = *_pointer;

        return true;
    }

    public static implicit operator NativePtr<T>(T value) => new NativePtr<T>(value);
    public static implicit operator T*(NativePtr<T> value) => value._pointer;
    public static implicit operator IntPtr(NativePtr<T> value) => (IntPtr)value._pointer;

    public static implicit operator NativePtr<T>(IntPtr value) => new NativePtr<T>(value);
}

public unsafe struct NativeVoidPtr : IDisposable
{
    private void* _pointer;

    public NativeVoidPtr(void* value, uint size)
    {
        _pointer = NativeMemory.Alloc(size);
        NativeMemory.Copy(value, _pointer, size);
        Size = size;
    }

    public static NativeVoidPtr Create<T>(T value) where T : unmanaged
    {
        return new NativeVoidPtr(&value, (uint) sizeof(T));;
    }

    public uint Size { get; private set; }

    public bool IsAllocated => _pointer != (void*)0x0;
    
    public void Free() => Dispose();

    public void Dispose()
    {
        if (_pointer == (void*)0x0)
            throw new NullReferenceException();
        
        NativeMemory.Free(_pointer);
        _pointer = (void*)0x0;
    }

    public ref T GetValue<T>() where T : unmanaged
    {
        if (!IsAllocated)
        {
            _pointer = NativeMemory.Alloc((uint) sizeof(T));

            T value = new T();
            uint size = (uint)sizeof(T);
            
            NativeMemory.Copy(&value, _pointer, size);
            Size = size;
        }
        
        return ref *(T*)_pointer;
    }
    
    public static implicit operator void*(NativeVoidPtr value) => value._pointer;
    public static implicit operator IntPtr(NativeVoidPtr value) => (IntPtr)value._pointer;
}