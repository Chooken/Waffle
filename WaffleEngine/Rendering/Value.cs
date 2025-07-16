namespace WaffleEngine.Rendering;

public class Value<T>(T value)
{
    private T _value = value;

    public T GetValue() => _value;
    public T SetValue(T value) => _value = value;

    public static implicit operator Value<T>(T value) => new Value<T>(value);
    public static implicit operator T(Value<T> value) => value._value;
}