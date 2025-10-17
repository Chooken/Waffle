namespace WaffleEngine;

public struct KeyState
{
    public bool KeyPressed;
    public bool KeyDown;
    public bool KeyRepeated;

    internal void Reset()
    {
        KeyPressed = false;
        KeyRepeated = false;
    }
}