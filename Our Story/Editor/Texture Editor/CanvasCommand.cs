using WaffleEngine;

namespace OurStory.Editor;

public struct CanvasDrawCommand : ICommand
{
    private Canvas _canvas;
    private Color _previousColor;
    private Color _newColor;
    private uint _drawX;
    private uint _drawY;
    
    public CanvasDrawCommand(Canvas canvas, Color color, uint x, uint y)
    {
        _canvas = canvas;
        _previousColor = canvas.GetColor(x, y);
        _newColor = color;
        _drawX = x;
        _drawY = y;
    }
    
    public void Undo()
    {
        _canvas.SetPixel(_previousColor, _drawX, _drawY);
    }

    public void Do()
    {
        _canvas.SetPixel(_newColor, _drawX, _drawY);
    }
}