using WaffleEngine;

namespace OurStory.Editor;

public interface ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition);
    public void OnMouseDown(Canvas canvas, Vector2 cursorPosition);
    public void OnHold(Canvas canvas, Vector2 cursorPosition);
    public void OnMouseUp(Canvas canvas, Vector2 cursorPosition);
}