namespace WaffleEngine.UI;

public interface ILayout
{
    /// <summary>
    /// Calculates the fit size of the element.
    /// </summary>
    /// <param name="element">The Element to calculate.</param>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void CalculateFitSize(UiElement element, bool width);

    /// <summary>
    /// Calculates the size of elements that depend on fit sized parents.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="width"></param>
    public void CalculatePercentages(UiElement element, bool width);

    /// <summary>
    /// Grows the children to fill the remainder.
    /// </summary>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void GrowChildren(UiElement element, bool width);

    /// <summary>
    /// Sets the Rects Position and Calculates Child Positions.
    /// </summary>
    /// <param name="position">The position of the element.</param>
    public void CalculatePositions(UiElement element, Vector2 position);
}