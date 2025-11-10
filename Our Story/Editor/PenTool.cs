using WaffleEngine;
using WaffleEngine.Rendering.Immediate;

namespace OurStory.Editor;

public class PenTool : ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition, Color color)
    {
        canvas.SetTempPixel(color, cursorPosition);
    }

    public void OnClick(Canvas canvas, Vector2 cursorPosition, Color color)
    {
        
    }

    public void OnHold(Canvas canvas, Vector2 cursorPosition, Color color)
    {
        canvas.SetPixel(color, (uint)cursorPosition.x, (uint)cursorPosition.y);
    }
}