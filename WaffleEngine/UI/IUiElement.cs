using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public interface IUiElement
{
    public ref IUiElement? Parent { get; }
    public ref UiSettings Settings { get; }
    public ref UiLayout Layout { get; }
    
    /// <summary>
    /// Calculates the fit size of the element.
    /// </summary>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void CalculateFitSize(bool width);
    
    /// <summary>
    /// Calculates the size of elements with percentage sizes.
    /// </summary>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void CalculatePercentages(bool width);
    
    /// <summary>
    /// Grows the children to fill the remainder.
    /// </summary>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void GrowOrShrink(bool width);
    
    /// <summary>
    /// Sets the Rects Position and Calculates Child Positions.
    /// </summary>
    /// <param name="position">The position of the element.</param>
    public void CalculatePositions(Vector2 position);

    /// <summary>
    /// Renders Elements in the render pass.
    /// </summary>
    /// <param name="renderPass">The render pass to render in.</param>
    /// <param name="renderSize">The Size of the Screen/Texture.</param>
    /// <param name="scale">The Display Scale</param>
    public void Render(ImRenderPass renderPass, Vector2 renderSize, float scale);

    /// <summary>
    /// Propagates a Update call.
    /// </summary>
    /// <param name="window">The window for events.</param>
    /// <param name="propagateEvents">Whether to propagate events.</param>
    /// <returns>If the function captured the event.</returns>
    public bool PropagateUpdate(Window window, bool propagateEvents);
}