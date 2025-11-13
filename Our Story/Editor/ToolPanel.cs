using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ToolPanel
{
    public Rect Panel;

    public Action<ICanvasTool> OnToolSelected;

    public Color BackgroundColor;
    public Color ButtonColor;
    public Color ButtonClickColor;

    public ToolPanel()
    {
        Panel = new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Grow,
                Color = BackgroundColor,
                Padding = 8,
                Gap = 4,
                BorderRadius = 8,
            });
        
        Panel.Add(ToolButton(new PenTool(), "Pen"));
        Panel.Add(ToolButton(new ShadeTool(), "Shade"));
    }

    public Rect ToolButton(ICanvasTool tool, string name) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Padding = (8, 4),
                Color = ButtonColor,
                BorderRadius = 4,
            })
            .OnClick((ref RectSettings settings) =>
            {
                OnToolSelected?.Invoke(tool);
            })
            .OnHold((ref RectSettings settings) =>
            {
                settings.Color = ButtonClickColor;
            });
            //.Add(new UIText(name)); Add Text

    public static implicit operator Rect(ToolPanel toolPanel) => toolPanel.Panel;
}