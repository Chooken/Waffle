using WaffleEngine;

namespace OurStory.Editor;

public class EraserTool : ICanvasTool
{
    private CommandGroup _commandGroup;
    private int _lastX;
    private int _lastY;
    
    public void OnHover(Canvas canvas, Vector2 cursorPosition)
    {
        canvas.SetTempPixel(new Color(0,0,0,0), cursorPosition);
    }

    public void OnMouseDown(Canvas canvas, Vector2 cursorPosition)
    {
        _commandGroup = new CommandGroup();
        _lastX = -1;
        _lastY = -1;
    }

    public void OnHold(Canvas canvas, Vector2 cursorPosition)
    {
        if ((int)cursorPosition.x == -1 || (int)cursorPosition.y == -1)
        {
            return;
        }
        
        if (_lastX == (int)cursorPosition.x && _lastY == (int)cursorPosition.y)
        {
            return;
        }

        _lastX = (int)cursorPosition.x;
        _lastY = (int)cursorPosition.y;
        
        _commandGroup.Do(
            new CanvasDrawCommand(
                canvas, 
                new Color(0,0,0,0), 
                (uint) cursorPosition.x,
                (uint) cursorPosition.y
            )
        );
    }

    public void OnMouseUp(Canvas canvas, Vector2 cursorPosition)
    {
        TextureEditor.CommandList.Add(_commandGroup);
    }
}