using System.Reflection;

namespace WaffleEngine;

public class EventSpace
{
    private Dictionary<Keycode, KeyState> _keyStates = new();

    internal void Update()
    {
        foreach (var key in _keyStates)
        {
            var keyState = key.Value;
            keyState.Reset();
            _keyStates[key.Key] = keyState;
        }
    }
    
    internal bool TriggerKeyEvent(KeyEvent keyEvent)
    {
        if (!_keyStates.TryGetValue(keyEvent.Key, out var keyState))
            keyState = new KeyState();

        switch (keyEvent.Type)
        {
            case KeyEventType.Started:
                keyState.KeyDown = true;
                keyState.KeyPressed = true;
                break;
            
            case KeyEventType.Repeated:
                keyState.KeyRepeated = true;
                break;
            
            case KeyEventType.Ended:
                keyState.KeyDown = false;
                keyState.KeyRepeated = false;
                keyState.KeyPressed = false;
                break;
        }

        _keyStates[keyEvent.Key] = keyState;
        
        return true;
    }

    public bool KeyPressed(Keycode key)
    {
        return _keyStates.TryGetValue(key, out var keyState) && keyState.KeyPressed;
    }

    public bool KeyDown(Keycode key)
    {
        return _keyStates.TryGetValue(key, out var keyState) && keyState.KeyDown;
    }

    public bool KeyUp(Keycode key)
    {
        return !_keyStates.TryGetValue(key, out var keyState) || !keyState.KeyDown;
    }

    public bool KeyRepeated(Keycode key)
    {
        return _keyStates.TryGetValue(key, out var keyState) && keyState.KeyRepeated;
    }
}