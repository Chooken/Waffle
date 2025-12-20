using WaffleEngine;
using WaffleEngine.Rendering.Immediate;

namespace OurStory.Editor;

public class PenTool : ICanvasTool
{
    private CommandGroup _commandGroup;
    private int _lastX;
    private int _lastY;
    
    public void OnHover(Canvas canvas, Vector2 cursorPosition)
    {
        float brightness = TextureEditor.SharedState.SelectedColor.V;
        
        if (Input.Mouse.MouseWheelTicksDelta != 0)
        {
            brightness = float.Clamp(brightness + Input.Mouse.MouseWheelTicksDelta / 32, 0, 1);
            TextureEditor.SharedState.SelectedColor.V = brightness;
        }
        
        canvas.SetTempPixel(TextureEditor.SharedState.SelectedColor, cursorPosition);
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
        
        Color color = TextureEditor.SharedState.SelectedColor;

        if (_lastX == (int)cursorPosition.x && _lastY == (int)cursorPosition.y)
        {
            return;
        }

        _lastX = (int)cursorPosition.x;
        _lastY = (int)cursorPosition.y;
        
        _commandGroup.Do(
            new CanvasDrawCommand(
                canvas,
                color,
                (uint)cursorPosition.x,
                (uint)cursorPosition.y
            )
        );
    }

    public void OnMouseUp(Canvas canvas, Vector2 cursorPosition)
    {
        TextureEditor.CommandList.Add(_commandGroup);
    }
}