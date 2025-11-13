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
        Bounds.CalculatedWidth *= scale;
        Bounds.CalculatedHeight *= scale;
        Bounds.CalculatedPosition *= scale;
        Bounds.ContentWidth *= scale;
        Bounds.ContentHeight *= scale;
        
        foreach (var child in Children)
        {
            child.PropagateScale(scale);
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
                OnClick();
            }
            else if (window.WindowInput.MouseData.IsLeftDown)
            {
                OnHold();
            }
            else
            {
                OnHover();
            }

            captured = true;
        }

        if (!settings.Equals(Settings))
        {
            Settings = settings;
        }

        return captured;
    }

    public abstract void Update();
    
    public abstract void OnHover();

    public abstract void OnClick();

    public abstract void OnHold();
}