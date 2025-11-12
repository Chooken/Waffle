using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public abstract class UiElement
{
    public UiElement? Parent;
    public List<UiElement> Children = new();
    public UiBounds Bounds;
    public UiSettings Settings;
    
    /// <summary>
    /// The layout system used for the children.
    /// Defaults to Ui.Flex.
    /// </summary>
    public ILayout Layout = Ui.Flex;
    
    public delegate void ActionRef<T>(ref T item);
    
    private Func<UiSettings>? _default;
    private ActionRef<UiSettings>? _onHoverEvent;
    private ActionRef<UiSettings>? _onClickEvent;
    private ActionRef<UiSettings>? _onHoldEvent;

    public abstract void Render(ImRenderPass renderPass, Vector2 renderSize, float scale);
    
    public bool PropagateUpdate(Window window, bool propagateEvents)
    {
        Settings = _default?.Invoke() ?? default;
        
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            propagateEvents = Children[i].PropagateUpdate(window, propagateEvents);
        }
        
        if (propagateEvents)
        {
            propagateEvents = !ProcessEvents(window);
        }

        return propagateEvents;
    }
    
    public bool ProcessEvents(Window window)
    {
        Vector2 b = new Vector2(Bounds.CalulatedPosition.x + Bounds.CalculatedWidth, Bounds.CalulatedPosition.y + Bounds.CalculatedHeight);
        Vector2 mousePos = window.WindowInput.MouseData.Position;
        
        UiSettings settings = Settings;

        bool captured = false;

        if (MathF.Min(Bounds.CalulatedPosition.x, b.x) < mousePos.x && MathF.Min(Bounds.CalulatedPosition.y, b.y) < mousePos.y &&
            MathF.Max(Bounds.CalulatedPosition.x, b.x) > mousePos.x && MathF.Max(Bounds.CalulatedPosition.y, b.y) > mousePos.y)
        {
            if (window.WindowInput.MouseData.IsLeftPressed && _onClickEvent is not null)
            {
                _onClickEvent.Invoke(ref settings);
            }
            else if (window.WindowInput.MouseData.IsLeftDown && _onHoldEvent is not null)
            {
                _onHoldEvent.Invoke(ref settings);
            }
            else if (_onHoverEvent is not null)
            {
                _onHoverEvent.Invoke(ref settings);
            }

            captured = true;
        }

        if (!settings.Equals(Settings))
        {
            Settings = settings;
        }

        return captured;
    }
    
    public UiElement Default(Func<UiSettings> defaultSettings)
    {
        _default += defaultSettings;
        return this;
    }
    
    public T Default<T>(Func<UiSettings> defaultSettings) where T : UiElement
    {
        _default += defaultSettings;
        return (T)this;
    }

    public UiElement OnHover(ActionRef<UiSettings> hover)
    {
        _onHoverEvent += hover;
        return this;
    }

    public T OnHover<T>(ActionRef<UiSettings> hover) where T : UiElement
    {
        _onHoverEvent += hover;
        return (T)this;
    }

    public UiElement OnClick(ActionRef<UiSettings> click)
    {
        _onClickEvent += click;
        return this;
    }

    public T OnClick<T>(ActionRef<UiSettings> click) where T : UiElement
    {
        _onClickEvent += click;
        return (T)this;
    }
    
    public UiElement OnHold(ActionRef<UiSettings> hold)
    {
        _onHoldEvent += hold;
        return this;
    }
    
    public T OnHold<T>(ActionRef<UiSettings> hold) where T : UiElement
    {
        _onHoldEvent += hold;
        return (T)this;
    }

    public UiElement Add(UiElement child)
    {
        Children.Add(child);
        child.Parent = this;
        return this;
    }
    
    public T Add<T>(UiElement child) where T : UiElement
    {
        Children.Add(child);
        child.Parent = this;
        return (T)this;
    }
}