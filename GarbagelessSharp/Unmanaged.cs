using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GarbagelessSharp;

public unsafe struct Unmanaged<T> : IDisposable where T : unmanaged
{
    private T* _pointer;
    
    public Unmanaged(T value)
    {
        _pointer = (T*) NativeMemory.Alloc((UIntPtr)sizeof(T));
        *_pointer = value;
    }
    
    public Unmanaged(IntPtr value)
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

    public ref T Value => ref *_pointer;

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = null;
        
        if (_pointer == (T*)0x0)
            return false;

        value = *_pointer;

        return true;
    }

    public static implicit operator Unmanaged<T>(T value) => new Unmanaged<T>(value);
    public static implicit operator T*(Unmanaged<T> value) => value._pointer;
    public static implicit operator IntPtr(Unmanaged<T> value) => (IntPtr)value._pointer;

    public static implicit operator Unmanaged<T>(IntPtr value) => new Unmanaged<T>(value);
}

public unsafe struct UnmanagedVoid : IDisposable
{
    private void* _pointer;

    public UnmanagedVoid(void* value, uint size)
    {
        _pointer = NativeMemory.Alloc(size);
        NativeMemory.Copy(value, _pointer, size);
        Size = size;
    }

    public static UnmanagedVoid Create<T>(T value) where T : unmanaged
    {
        return new UnmanagedVoid(&value, (uint) sizeof(T));;
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
    
    public static implicit operator void*(UnmanagedVoid value) => value._pointer;
    public static implicit operator IntPtr(UnmanagedVoid value) => (IntPtr)value._pointer;
}