namespace WaffleEngine.UI;

public interface IUiElement
{
    public void CalculateFitSize();
    public void GrowOrShrink();
    public void CalculatePosition();
    public void Render();
}