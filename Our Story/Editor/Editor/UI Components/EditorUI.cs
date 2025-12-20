using WaffleEngine.UI;

namespace OurStory.Editor.UI_Components;

public class EditorUI : Rect
{
    public EditorUI()
    {
        Default((ref RectSettings settings) =>
        {
            settings.Width = Ui.Grow;
            settings.Height = Ui.Grow;
            settings.Padding = 8;
        });
        
        Add(new Rect()
            .Default((ref RectSettings settings) =>
            {
                settings.Width = Ui.Grow;
                settings.Height = Ui.Grow;
                settings.Color = Editor.PanelColor;
                settings.BorderRadius = 8;
            })
        );
    }
}