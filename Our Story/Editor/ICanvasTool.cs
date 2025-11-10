using WaffleEngine;

namespace OurStory.Editor;

public interface ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition, Color color);
    public void OnClick(Canvas canvas, Vector2 cursorPosition, Color color);
    public void OnHold(Canvas canvas, Vector2 cursorPosition, Color color);
}