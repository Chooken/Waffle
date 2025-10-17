namespace WaffleEngine;

public struct KeyEvent
{
    public KeyEventType Type;
    public Keycode Key;
}

public enum KeyEventType
{
    Started,
    Repeated,
    Ended,
}