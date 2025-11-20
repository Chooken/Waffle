using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ColorPanel : Rect
{
    public ColorPanel()
    {
        Default(() => new RectSettings()
        {
            Height = Ui.Grow,
            Color = TextureEditor.BackgroundColor,
            Direction = UiDirection.TopToBottom,
            Padding = 8,
            BorderRadius = 8,
        });

        int rows = 17;
        int columns = 7;
        
        for (int y = 0; y < rows - 1; y++)
        {
            var rect = new Rect()
                .Default(() => new RectSettings()
                {
                    Gap = 4,
                    Padding = new UiPadding() { Bottom = 4 },
                });
            
            for (int x = 0; x < columns; x++)
            {
                OklabColor color = OklabColor.FromLCH((1f - (float)x / columns) * 0.7f + 0.3f, (1f - (float)x / columns) * 0.125f, (float)y / rows * Single.Pi * 2);
                
                rect.Add(ColorToggle(color));
            }

            Add(rect);
        }
        
        var finalRect = new Rect()
            .Default(() => new RectSettings()
            {
                Gap = 4,
            });

        finalRect.Add(ColorToggle(new Color(1, 1, 1, 1)));
        finalRect.Add(ColorToggle(new Color(0, 0, 0, 1)));
        finalRect.Add(ColorToggle(new Color(0, 0, 0, 0)));

        Add(finalRect);
    }
    
    public Rect ColorToggle(Color color) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Fixed(18),
                Height = Ui.Fixed(18),
                Color = color.WithAlphaOne(),
                BorderColor = new Color(1,1,1,1),
                BorderSize = TextureEditor.SharedState.SelectedColor == color ? 4 : 0,
            })
            .OnHover((ref RectSettings settings) =>
            {
                if (TextureEditor.SharedState.SelectedColor == color)
                    return;
                
                settings.BorderColor = new Color(0, 0, 0, 1);
                settings.BorderSize = 4;
            })
            .OnClick((ref RectSettings settings) =>
            {
                TextureEditor.SharedState.SelectedColor = color;
                TextureEditor.SharedState.ColorBrightness = 1;
            });
}