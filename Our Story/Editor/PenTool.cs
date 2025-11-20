using WaffleEngine;
using WaffleEngine.Rendering.Immediate;

namespace OurStory.Editor;

public class PenTool : ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition)
    {
        Color color = TextureEditor.SharedState.SelectedColor;
        ref float brightness = ref TextureEditor.SharedState.ColorBrightness;
        
        if (Input.Mouse.MouseWheelTicksDelta != 0)
        {
            brightness = float.Clamp(brightness + (float)Input.Mouse.MouseWheelTicksDelta / 64, 0, 1);
        }

        color.r *= brightness;
        color.g *= brightness;
        color.b *= brightness;
        
        canvas.SetTempPixel(color, cursorPosition);
    }

    public void OnClick(Canvas canvas, Vector2 cursorPosition)
    {
        
    }

    public void OnHold(Canvas canvas, Vector2 cursorPosition)
    {
        Color color = TextureEditor.SharedState.SelectedColor;
        float brightness = TextureEditor.SharedState.ColorBrightness;
        
        color.r *= brightness;
        color.g *= brightness;
        color.b *= brightness;
        
        TextureEditor.CommandList.Do(
            new CanvasDrawCommand(
                canvas, 
                color,
                (uint)cursorPosition.x,
                (uint)cursorPosition.y
            )
        );
    }
}