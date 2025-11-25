using WaffleEngine;
using WaffleEngine.Text;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ToolSelector : Rect
{
    public ToolSelector()
    {
        Default(() => new RectSettings()
        {
            Height = Ui.Grow,
            Color = TextureEditor.BackgroundColor,
            Padding = 4,
            Gap = 4,
            BorderRadius = 8,
            Alignment = new UiAlignment
            {
                Vertical = UiAlignmentVertical.Center
            }
        });

        foreach (var type in TextureEditor.SharedState.Tools.Keys)
        {
            Add(ToolButton(TextureEditor.SharedState.Tools[type], TextureEditor.Font, type.Name[..^4]));
        }
    }

    public Rect ToolButton(ICanvasTool tool, Font font, string name) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Height = Ui.Grow,
                Padding = (8, 4),
                Color = TextureEditor.SharedState.SelectedTool == tool ? 
                    TextureEditor.PanelColor : 
                    Color.Transparent,
                BorderRadius = 6,
                Alignment = new UiAlignment
                {
                    Vertical = UiAlignmentVertical.Center,
                    Horizontal = UiAlignmentHorizontal.Center,
                },
                Gap = 8,
            })
            .OnMouseDown((ref RectSettings settings) =>
            {
                TextureEditor.SharedState.SelectedTool = tool;
            })
            .OnHover((ref RectSettings settings) =>
            {
                settings.Cursor = Cursor.Pointer;
                settings.Color = TextureEditor.PanelColor;
            })
            .Add(new Rect()
                .Default(() => new RectSettings
                {
                    Width = Ui.Pixels(10),
                    Height = Ui.Pixels(10),
                    BorderRadius = 5,
                    Color = TextureEditor.SharedState.SelectedTool == tool ? 
                        TextureEditor.Highlight : 
                        TextureEditor.FontColor
                }))
            .Add(new Text(font)
                .Default(() => new Text.TextSettings
                {
                    Text = name,
                    Color = TextureEditor.SharedState.SelectedTool == tool ? 
                        TextureEditor.Highlight : 
                        TextureEditor.FontColor,
                    Size = 14,
                }));

}