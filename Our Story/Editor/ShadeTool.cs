using WaffleEngine;

namespace OurStory.Editor;

public class ShadeTool : ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition, Color color)
    {
        if (Input.Mouse.MouseWheelTicksDelta < 0)
        {
            var pixelColor = canvas.GetColor((uint)cursorPosition.x, (uint)cursorPosition.y);
            pixelColor.r = MathF.Max(pixelColor.r - 8f / 255, 0);
            pixelColor.g = MathF.Max(pixelColor.g - 8f / 255, 0);
            pixelColor.b = MathF.Max(pixelColor.b - 8f / 255, 0);
            
            canvas.SetPixel(pixelColor, (uint)cursorPosition.x, (uint)cursorPosition.y);
        }
    }

    public void OnClick(Canvas canvas, Vector2 cursorPosition, Color color)
    {
        
    }

    public void OnHold(Canvas canvas, Vector2 cursorPosition, Color color)
    {
        
    }
}