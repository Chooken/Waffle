using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class CanvasPanel
{
    public UIRect PanelUI;
    public UICrt CanvasUI;
    public Canvas Canvas;
    public Color CursorColor;
    public Vector2 CursorPosition;
    public Shader CanvasBlitShader;
    public ICanvasTool? CanvasTool;
    
    public CanvasPanel(Window window, uint width, uint height)
    {
        Canvas = new Canvas(width, height);
        CanvasUI = new UICrt(new Vector2(width, height), 0.0f);
        PanelUI = new UIRect()
            .Default(() => new UISettings()
            {
                Height = UISize.PercentageHeight(100),
                Grow = true,
                ChildAnchor = UIAnchor.Center,
                Color = Color.RGBA255(255,0,0,255),
                BorderRadius = new UIBorderRadius(20, 20, 20, 20, UISizeType.Pixels)
            })
            .AddUIElement(CanvasUI
                .Default(() =>
                {
                    CursorPosition = new Vector2(-1, -1);
                    
                    return new UISettings()
                    {
                        Width = UISize.PercentageWidth(100),
                        Height = UISize.PercentageHeight(100),
                        BorderRadius = new UIBorderRadius(5, 5, 5, 5, UISizeType.PercentageWidth),
                        BorderSize = UISize.Pixels(4),
                        BorderColor = Color.RGBA255(22, 22, 22, 255),
                        Texture = Canvas.GetCanvas(),
                    };
                })
                .OnHover((ref UISettings settings) =>
                {
                    CalculateCursorPosition(window);
                    CanvasTool?.OnHover(Canvas, CursorPosition, CursorColor);
                })
                .OnClick((ref UISettings item) =>
                {
                    CalculateCursorPosition(window);
                    CanvasTool?.OnClick(Canvas, CursorPosition, CursorColor);
                })
                .OnHold((ref UISettings settings) =>
                {
                    CalculateCursorPosition(window);
                    CanvasTool?.OnHold(Canvas, CursorPosition, CursorColor);
                })
            );
    }

    private void CalculateCursorPosition(Window window)
    {
        Vector2 position = window.WindowInput.MouseData.Position;
        Vector3 uiPos = CanvasUI.LastPosition;
        Vector2 uiSize = CanvasUI.LastSize;

        CursorPosition = new Vector2(
            (int)((position.x - uiPos.x) / uiSize.x * CanvasUI.Resolution.x),
            (int)((position.y - uiPos.y) / uiSize.y * CanvasUI.Resolution.y));
    }

    public void RenderCanvas(ref ImQueue queue)
    {
        Canvas.Render(ref queue);
    }

    public static implicit operator UIRect(CanvasPanel panel) => panel.PanelUI;
}