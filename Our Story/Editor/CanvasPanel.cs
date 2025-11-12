using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class CanvasPanel
{
    public Rect PanelUI;
    public RectCrt CanvasRect;
    public Canvas Canvas;
    public Color CursorColor;
    public Vector2 CursorPosition;
    public Shader CanvasBlitShader;
    public ICanvasTool? CanvasTool;
    
    public CanvasPanel(Window window, uint width, uint height)
    {
        Canvas = new Canvas(width, height);
        CanvasRect = new RectCrt(Canvas.GetCanvas(), new Vector2(width, height), 0.0f);
        PanelUI = new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Grow,
                Height = Ui.Grow,
                Alignment = (UiAlignmentVertical.Center, UiAlignmentHorizontal.Center),
                Color = Color.RGBA255(255,0,0,255),
                BorderRadius = 20,
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
                        BorderColor = Color.RGBA255(22, 22, 22, 255),
                    };
                })
                .OnHover((ref RectSettings settings) =>
                {
                    CalculateCursorPosition(window);
                    CanvasTool?.OnHover(Canvas, CursorPosition, CursorColor);
                })
                .OnClick((ref RectSettings item) =>
                {
                    CalculateCursorPosition(window);
                    CanvasTool?.OnClick(Canvas, CursorPosition, CursorColor);
                })
                .OnHold((ref RectSettings settings) =>
                {
                    CalculateCursorPosition(window);
                    CanvasTool?.OnHold(Canvas, CursorPosition, CursorColor);
                })
            );
    }

    private void CalculateCursorPosition(Window window)
    {
        Vector2 position = window.WindowInput.MouseData.Position;
        Vector3 uiPos = CanvasRect.Bounds.CalulatedPosition;
        Vector2 uiSize = new Vector2(CanvasRect.Bounds.CalculatedWidth, CanvasRect.Bounds.CalculatedHeight);

        CursorPosition = new Vector2(
            (int)((position.x - uiPos.x) / uiSize.x * CanvasRect.Resolution.x),
            (int)((position.y - uiPos.y) / uiSize.y * CanvasRect.Resolution.y));
    }

    public void RenderCanvas(ref ImQueue queue)
    {
        Canvas.Render(ref queue);
    }

    public static implicit operator Rect(CanvasPanel panel) => panel.PanelUI;
}