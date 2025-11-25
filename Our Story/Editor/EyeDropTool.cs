using WaffleEngine;

namespace OurStory.Editor;

public class EyeDropTool : ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition)
    {
        
    }

    public void OnMouseDown(Canvas canvas, Vector2 cursorPosition)
    {
        
    }

    public void OnHold(Canvas canvas, Vector2 cursorPosition)
    {
        
    }

    public void OnMouseUp(Canvas canvas, Vector2 cursorPosition)
    {
        TextureEditor.SharedState.SelectColor(canvas.GetColor((uint)cursorPosition.x, (uint)cursorPosition.y));
        TextureEditor.SharedState.SelectTool<PenTool>();
    }
}