using WaffleEngine;
using WaffleEngine.UI.Old;

namespace OurStory.Editor;

public class ToolPanel
{
    public UIRect Panel = new UIRect()
        .Default(() => new UISettings()
        {
            Width = UISize.PercentageWidth(100),
            Color = Color.RGBA255(22, 22, 22, 255),
            PaddingX = UISize.Pixels(8),
            PaddingY = UISize.Pixels(8),
            Gap = UISize.Pixels(4),
            BorderRadius = new UIBorderRadius(8, 8, 8, 8, UISizeType.Pixels),
        });

    public Action<ICanvasTool> OnToolSelected;

    public ToolPanel()
    {
        Panel.AddUIElement(ToolButton(new PenTool(), "Pen"));
        Panel.AddUIElement(ToolButton(new ShadeTool(), "Shade"));
    }
    
    public UIRect ToolButton(ICanvasTool tool, string name) =>
        new UIRect()
            .Default(() => new UISettings()
            {
                PaddingX = UISize.Pixels(8),
                PaddingY = UISize.Pixels(4),
                Color = Color.RGBA255(40, 40, 40, 255),
                BorderRadius = new UIBorderRadius(4, 4, 4, 4, UISizeType.Pixels),
            })
            .OnClick((ref UISettings settings) =>
            {
                OnToolSelected?.Invoke(tool);
            })
            .OnHold((ref UISettings settings) =>
            {
                settings.Color = Color.RGBA255(30, 30, 30, 255);
            })
            .AddUIElement(new UIText()
                .SetText(name));

    public static implicit operator UIRect(ToolPanel toolPanel) => toolPanel.Panel;
}