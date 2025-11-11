using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public interface IUiElement
{
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
    public void Render(ImRenderPass renderPass, Vector2 renderSize);
}