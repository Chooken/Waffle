using WaffleEngine.UI;

namespace OurStory.Editor;

public class Topbar : Rect
{
    public Topbar()
    {
        Default((ref RectSettings settings) => settings = settings with
        {
            Width = Ui.Grow,
            Padding = 4,
            BorderRadius = 8,
            Color = TextureEditor.PanelColor,
            Gap = 4,
            Alignment = new UiAlignment
            {
                Vertical = UiAlignmentVertical.Center,
            }
        });
        
        Add(new ToolSelector());
        Add(new Spacer());
        Add(new UndoButton());
        Add(new RedoButton());
    }
}