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
            Color = TextureEditor.PanelColor,
            Direction = UiDirection.TopToBottom,
            Padding = (8, 12),
            BorderRadius = 8,
            Gap = 12,
        });

        Add(new Rect()
            .Default(() => new RectSettings
            {
                Width = Ui.Grow,
                Padding = (2, 0),
            })
            .Add(new Text("Palette", TextureEditor.Font)
                .Default(() => new Text.TextSettings
                {
                    Color = TextureEditor.FontColor,
                    Size = 12,
                })
            )
            .Add(new Rect()
                .Default(() => new RectSettings
                {
                    Width = Ui.Grow,
                })
            )
            .Add(new Rect()
                .Default(() => new RectSettings
                {
                    
                })
            )
        );
        

        Rect colorSelector = new Rect()
            .Default(() => new RectSettings
            {
                Direction = UiDirection.TopToBottom,
                Gap = 8,
            });

        int rows = 4;
        int columns = 7;
        
        for (int y = 0; y < rows - 1; y++)
        {
            var rect = new Rect()
                .Default(() => new RectSettings()
                {
                    Gap = 8
                });
            
            for (int x = 0; x < columns; x++)
            {
                OklabColor color = OklabColor.FromLCH(0.85f, 0.085f, (float)(x + y * columns) / (rows * columns + columns) * Single.Pi * 3);
                
                rect.Add(ColorToggle(color));
            }

            colorSelector.Add(rect);
        }
        
        var finalRect = new Rect()
            .Default(() => new RectSettings()
            {
                Gap = 8,
            });

        finalRect.Add(ColorToggle(new Color(1, 1, 1, 1)));
        finalRect.Add(ColorToggle(new Color(0, 0, 0, 1)));

        colorSelector.Add(finalRect);

        Add(colorSelector);
    }
    
    public Rect ColorToggle(Color color) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Fixed(20),
                Height = Ui.Fixed(20),
                Color = color.WithAlphaOne(),
                BorderRadius = 10,
                BorderColor = new Color(1,1,1,1),
                BorderSize = TextureEditor.SharedState.SelectedColor == color ? 4 : 0,
            })
            .OnHover((ref RectSettings settings) =>
            {
                settings.Cursor = Cursor.Pointer;
                
                if (TextureEditor.SharedState.SelectedColor == color)
                    return;
                
                settings.BorderColor = new Color(1,1,1,1);
                settings.BorderSize = 4;
            })
            .OnMouseDown((ref RectSettings settings) =>
            {
                TextureEditor.SharedState.SelectedColor = color;
                TextureEditor.SharedState.ColorBrightness = 1;
                TextureEditor.SharedState.SelectedTool = 
                    TextureEditor.SharedState.Tools[typeof(PenTool)];
            });
}