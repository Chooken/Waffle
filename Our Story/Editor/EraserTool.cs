using WaffleEngine;

namespace OurStory.Editor;

public class EraserTool : ICanvasTool
{
    public void OnHover(Canvas canvas, Vector2 cursorPosition)
    {
        canvas.SetTempPixel(new Color(0,0,0,0), cursorPosition);
    }

    public void OnClick(Canvas canvas, Vector2 cursorPosition)
    {
        
    }

    public void OnHold(Canvas canvas, Vector2 cursorPosition)
    {
        TextureEditor.CommandList.Do(
            new CanvasDrawCommand(
                canvas, 
                new Color(0,0,0,0), 
                (uint) cursorPosition.x,
                (uint) cursorPosition.y
            )
        );
    }
}