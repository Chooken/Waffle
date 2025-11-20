using WaffleEngine;

namespace OurStory.Editor;

public interface ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition);
    public void OnClick(Canvas canvas, Vector2 cursorPosition);
    public void OnHold(Canvas canvas, Vector2 cursorPosition);
}