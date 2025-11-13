using System.Data;
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

    public void PropagateScale(float scale)
    {
        Bounds.Scale = scale;
        
        foreach (var child in Children)
        {
            child.PropagateScale(scale);
        }
    }

    public void ColapseScale()
    {
        Bounds.CalculatedWidth *= Bounds.Scale;
        Bounds.CalculatedHeight *= Bounds.Scale;
        Bounds.CalculatedPosition *= Bounds.Scale;
        Bounds.ContentWidth *= Bounds.Scale;
        Bounds.ContentHeight *= Bounds.Scale;

        foreach (var child in Children)
        {
            child.ColapseScale();
        }
    }

    public void PropagateRender(ImRenderPass renderPass, Vector2 renderSize)
    {
        Render(renderPass, renderSize);
        
        foreach (var child in Children)
        {
            child.PropagateRender(renderPass, renderSize);
        }
    }

    public abstract void Render(ImRenderPass renderPass, Vector2 renderSize);
    
    public bool PropagateUpdate(Window window, bool propagateEvents)
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            propagateEvents = Children[i].PropagateUpdate(window, propagateEvents);
        }
        
        if (propagateEvents)
        {
            propagateEvents = !ProcessEvents(window);
        }
        
        Update();

        return propagateEvents;
    }
    
    public bool ProcessEvents(Window window)
    {
        Vector2 b = new Vector2(Bounds.CalculatedPosition.x + Bounds.CalculatedWidth, Bounds.CalculatedPosition.y + Bounds.CalculatedHeight);
        Vector2 mousePos = window.WindowInput.MouseData.Position;
        
        UiSettings settings = Settings;

        bool captured = false;

        if (MathF.Min(Bounds.CalculatedPosition.x, b.x) < mousePos.x && MathF.Min(Bounds.CalculatedPosition.y, b.y) < mousePos.y &&
            MathF.Max(Bounds.CalculatedPosition.x, b.x) > mousePos.x && MathF.Max(Bounds.CalculatedPosition.y, b.y) > mousePos.y)
        {
            if (window.WindowInput.MouseData.IsLeftPressed)
            {
                captured = OnClick();
            }
            else if (window.WindowInput.MouseData.IsLeftDown)
            {
                captured = OnHold();
            }
            else
            {
                captured = OnHover();
            }
        }

        if (!settings.Equals(Settings))
        {
            Settings = settings;
        }

        return captured;
    }

    public abstract void Update();
    
    /// <summary>
    /// Calls when the mouse is hovering the element.
    /// </summary>
    /// <returns>True if the element captures the event.</returns>
    public abstract bool OnHover();

    /// <summary>
    /// Calls when the mouse clicked the element.
    /// </summary>
    /// <returns>True if the element captures the event.</returns>
    public abstract bool OnClick();

    /// <summary>
    /// Calls when the mouse holds the click down on the element.
    /// </summary>
    /// <returns>True if the element captures the event.</returns>
    public abstract bool OnHold();
}