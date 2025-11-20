using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class CanvasPanel : Rect
{
    public RectCrt CanvasRect;
    public Canvas Canvas;
    public Vector2 CursorPosition;
    
    public CanvasPanel(Window window, Canvas canvas)
    {
        Canvas = canvas;
        CanvasRect = new RectCrt(Canvas.GetCanvas(), 0.0f);
        
        Default(() => new RectSettings()
        {
            Width = Ui.Grow,
            Height = Ui.Grow,
            Alignment = (UiAlignmentVertical.Center, UiAlignmentHorizontal.Center),
        })
        .Add(CanvasRect
            .Default(() =>
            {
                CursorPosition = new Vector2(-1, -1);
                
                return new RectSettings()
                {
                    Width = Ui.Grow,
                    Height = Ui.Grow,
                    BorderRadius = 40,
                    BorderSize = 4,
                    BorderColor = TextureEditor.PanelColor,
                };
            })
            .OnHover((ref RectSettings settings) =>
            {
                CalculateCursorPosition(window);
                TextureEditor.SharedState.SelectedTool?.OnHover(Canvas, CursorPosition);
            })
            .OnClick((ref RectSettings item) =>
            {
                CalculateCursorPosition(window);
                TextureEditor.SharedState.SelectedTool?.OnClick(Canvas, CursorPosition);
            })
            .OnHold((ref RectSettings settings) =>
            {
                CalculateCursorPosition(window);
                TextureEditor.SharedState.SelectedTool?.OnHold(Canvas, CursorPosition);
            })
        );
    }

    private void CalculateCursorPosition(Window window)
    {
        Vector2 position = window.WindowInput.MouseData.Position;
        Vector3 uiPos = CanvasRect.Bounds.CalculatedPosition;
        Vector2 uiSize = new Vector2(CanvasRect.Bounds.CalculatedWidth, CanvasRect.Bounds.CalculatedHeight);

        CursorPosition = new Vector2(
            (int)((position.x - uiPos.x) / uiSize.x * CanvasRect.Texture.Width),
            (int)((position.y - uiPos.y) / uiSize.y * CanvasRect.Texture.Height));
    }
}