namespace WaffleEngine;

public sealed class InputHandler
{
    private EventSpace _defaultEventSpace = new();
    private HashSet<Keycode> _activeKeys = new();
    private Dictionary<Modifier, EventSpace> _modifiers = new(new ModifierComparer());

    internal void Update()
    {
        _defaultEventSpace.Update();

        foreach (var eventSpace in _modifiers.Values)
        {
            eventSpace.Update();
        }
    }

    internal bool TriggerKeyEvent(Keycode key, bool isPressed)
    {
        KeyEventType eventType = KeyEventType.Ended;
        
        if (isPressed)
        {
            if (_activeKeys.Contains(key))
                eventType = KeyEventType.Repeated;
            else
            {
                eventType = KeyEventType.Started;
            }
        }

        KeyEvent keyEvent = new KeyEvent
        {
            Key = key,
            Type = eventType,
        };

        if (_modifiers.GetAlternateLookup<HashSet<Keycode>>().TryGetValue(_activeKeys, out var eventSpace))
        {
            if (isPressed)
            {
                _activeKeys.Add(key);
            }
            else
            {
                _activeKeys.Remove(key);
            }

            eventSpace.TriggerKeyEvent(keyEvent);
        }
        
        if (isPressed)
        {
            _activeKeys.Add(key);
        }
        else
        {
            _activeKeys.Remove(key);
        }
        
        return _defaultEventSpace.TriggerKeyEvent(keyEvent);
    }

    public EventSpace DefaultEventSpace => _defaultEventSpace;

    public EventSpace GetEventSpace(Modifier modifer)
    {
        if (_modifiers.TryGetValue(modifer, out var eventSpace))
            return eventSpace;

        eventSpace = new EventSpace();
        
        _modifiers.Add(modifer, eventSpace);
        return eventSpace;
    }

    public void AddEventSpace(Modifier modifier)
    {
        _modifiers.Add(modifier, new EventSpace());
    }
}
