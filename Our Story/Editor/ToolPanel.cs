using WaffleEngine;
using WaffleEngine.Text;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ToolPanel : Rect
{
    public ToolPanel(Font font)
    {
        Default(() => new RectSettings()
        {
            Width = Ui.Grow,
            Color = TextureEditor.PanelColor,
            Padding = 8,
            Gap = 4,
            BorderRadius = 8,
        });

        foreach (var type in TextureEditor.SharedState.Tools.Keys)
        {
            Add(ToolButton(TextureEditor.SharedState.Tools[type], font, type.Name[..^4]));
        }
    }

    public Rect ToolButton(ICanvasTool tool, Font font, string name) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Padding = (8, 4),
                Color = TextureEditor.ElementColor,
                BorderRadius = 4,
                BorderSize = TextureEditor.SharedState.SelectedTool == tool ? 4 : 0,
                BorderColor = TextureEditor.ElementHighlight,
            })
            .OnClick((ref RectSettings settings) =>
            {
                TextureEditor.SharedState.SelectedTool = tool;
            })
            .OnHold((ref RectSettings settings) =>
            {
                settings.Color = TextureEditor.ElementPressColor;
            })
            .Add(new Text(name, font));

}