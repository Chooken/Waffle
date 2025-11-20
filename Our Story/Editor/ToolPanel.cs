using WaffleEngine;
using WaffleEngine.Text;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ToolPanel : Rect
{
    public ToolPanel()
    {
        Default(() => new RectSettings()
        {
            Width = Ui.Grow,
            Padding = 4,
            BorderRadius = 8,
            Color = TextureEditor.PanelColor,
            Gap = 8,
        });

        var undoRedoPanel = new Rect()
            .Default(() => new RectSettings()
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

        undoRedoPanel.Add(new UndoButton());
        undoRedoPanel.Add(new RedoButton());

        var toolsPanel = new Rect()
            .Default(() => new RectSettings()
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
            toolsPanel.Add(ToolButton(TextureEditor.SharedState.Tools[type], TextureEditor.Font, type.Name[..^4]));
        }

        Add(undoRedoPanel);
        Add(toolsPanel);
    }

    public Rect ToolButton(ICanvasTool tool, Font font, string name) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Height = Ui.Grow,
                Padding = (12, 6),
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
                settings.Color = TextureEditor.PanelColor;
            })
            .Add(new Rect()
                .Default(() => new RectSettings
                {
                    Width = Ui.Fixed(10),
                    Height = Ui.Fixed(10),
                    BorderRadius = 5,
                    Color = TextureEditor.SharedState.SelectedTool == tool ? 
                        TextureEditor.Highlight : 
                        TextureEditor.FontColor
                }))
            .Add(new Text(name, font)
                .Default(() => new Text.TextSettings
                {
                    Color = TextureEditor.SharedState.SelectedTool == tool ? 
                        TextureEditor.Highlight : 
                        TextureEditor.FontColor
                }));

}