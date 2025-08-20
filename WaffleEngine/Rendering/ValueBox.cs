using System.Numerics;

namespace WaffleEngine.Rendering;

public sealed class ValueBox<T>(T value)
{
    private T _value = value;

    public ref T Value => ref _value;
    
    public T SetValue(T value) => _value = value;

    public static implicit operator ValueBox<T>(T value) => new ValueBox<T>(value);
    public static implicit operator T(ValueBox<T> valueBox) => valueBox._value;
}