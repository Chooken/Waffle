using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ToolPanel
{
    public Rect Panel = new Rect()
        .Default(() => new RectSettings()
        {
            Width = Ui.Grow,
            Color = Color.RGBA255(22, 22, 22, 255),
            Padding = 8,
            Gap = 4,
            BorderRadius = 8,
        });

    public Action<ICanvasTool> OnToolSelected;

    public ToolPanel()
    {
        Panel.Add(ToolButton(new PenTool(), "Pen"));
        Panel.Add(ToolButton(new ShadeTool(), "Shade"));
    }

    public Rect ToolButton(ICanvasTool tool, string name) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Padding = (8, 4),
                Color = Color.RGBA255(40, 40, 40, 255),
                BorderRadius = 4,
            })
            .OnClick((ref RectSettings settings) =>
            {
                OnToolSelected?.Invoke(tool);
            })
            .OnHold((ref RectSettings settings) =>
            {
                settings.Color = Color.RGBA255(30, 30, 30, 255);
            });
            //.Add(new UIText(name)); Add Text

    public static implicit operator Rect(ToolPanel toolPanel) => toolPanel.Panel;
}