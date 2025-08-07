using System.Diagnostics.CodeAnalysis;
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