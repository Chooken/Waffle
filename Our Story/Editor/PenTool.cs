using WaffleEngine;
using WaffleEngine.Rendering.Immediate;

namespace OurStory.Editor;

public class PenTool : ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition, ref Color color)
    {
        if (Input.Mouse.MouseWheelTicksDelta < 0)
        {
            color.r = MathF.Max(color.r - 8f / 255, 0);
            color.g = MathF.Max(color.g - 8f / 255, 0);
            color.b = MathF.Max(color.b - 8f / 255, 0);
        }
        
        canvas.SetTempPixel(color, cursorPosition);
    }

    public void OnClick(Canvas canvas, Vector2 cursorPosition, ref Color color)
    {
        
    }

    public void OnHold(Canvas canvas, Vector2 cursorPosition, ref Color color)
    {
        canvas.SetPixel(color, (uint)cursorPosition.x, (uint)cursorPosition.y);
    }
}