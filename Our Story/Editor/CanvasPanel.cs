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
    public int Zoom = 0;

    private float[] _zooms = [float.MaxValue, 512, 256, 128, 64, 32];
    
    public CanvasPanel(Window window, Canvas canvas)
    {
        Canvas = canvas;
        CanvasRect = new RectCrt(Canvas.GetCanvas(), 0.0f);

        Default((ref RectSettings settings) => settings = new RectSettings()
        {
            Width = Ui.Grow,
            Height = Ui.Grow,
            Alignment = (UiAlignmentVertical.Center, UiAlignmentHorizontal.Center),
        });
        
        Add(CanvasRect
            .Default((ref RectSettings settings) => 
            {
                Zoom = Math.Clamp(Zoom, 0, _zooms.Length - 1);
                CursorPosition = new Vector2(-1, -1);
                CanvasRect.UseMinMax = TextureEditor.SharedState.UseMinMax;
                settings = new RectSettings
                {
                    Width = Ui.Grow.Max(_zooms[Zoom]),
                    Height = Ui.Grow,
                    AspectRatio = (float)canvas.Width / canvas.Height,
                    BorderRadius = float.Min(42, _zooms[Zoom] * 0.2f),
                    BorderSize = 4,
                    BorderColor = TextureEditor.PanelColor,
                    Color = TextureEditor.BackgroundColor,
                };
            })
            .OnHover((ref RectSettings settings) =>
            {
                CalculateCursorPosition(window);
                TextureEditor.SharedState.SelectedTool?.OnHover(Canvas, CursorPosition);
            })
            .OnMouseDown((ref RectSettings settings) =>
            {
                CalculateCursorPosition(window);
                TextureEditor.SharedState.SelectedTool?.OnMouseDown(Canvas, CursorPosition);
            })
            .OnHold((ref RectSettings settings) =>
            {
                CalculateCursorPosition(window);
                TextureEditor.SharedState.SelectedTool?.OnHold(Canvas, CursorPosition);
            })
            .OnMouseUp((ref RectSettings settings) =>
            {
                TextureEditor.SharedState.SelectedTool?.OnMouseUp(Canvas, CursorPosition);
            })
        );

        Add(new Rect()
            .Default((ref RectSettings settings) =>
            {
                settings.Position = Ui.Relative.Right(4).Bottom(4);
                settings.Color = TextureEditor.PanelColor;
                settings.Padding = (8, 4);
                settings.BorderRadius = 4;
            })
            .Add(new Text(TextureEditor.Font)
                .Default(() => new Text.TextSettings
                {
                    Text = Zoom == 0 ? "auto" : $"{_zooms[Math.Clamp(Zoom, 0, _zooms.Length - 1)]}px",
                }))
        );
    }

    public override void Update()
    {
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Minus))
        {
            Zoom = Math.Min(_zooms.Length - 1, Zoom + 1);
        }
        else if (Input.GetDefaultEventSpace.KeyPressed(Keycode.Equals))
        {
            Zoom = Math.Max(0, Zoom - 1);
        }
        
        base.Update();
    }

    private void CalculateCursorPosition(Window window)
    {
        Vector2 position = window.WindowInput.MouseData.Position;
        Vector3 uiPos = CanvasRect.Bounds.CalculatedPosition;
        Vector2 uiSize = new Vector2(CanvasRect.Bounds.CalculatedWidth, CanvasRect.Bounds.CalculatedHeight);

        CursorPosition = new Vector2(
            int.Clamp((int)((position.x - uiPos.x) / uiSize.x * CanvasRect.Texture.Width), 0,  (int)CanvasRect.Texture.Width - 1),
            int.Clamp((int)((position.y - uiPos.y) / uiSize.y * CanvasRect.Texture.Height), 0,  (int)CanvasRect.Texture.Width - 1));
    }
}