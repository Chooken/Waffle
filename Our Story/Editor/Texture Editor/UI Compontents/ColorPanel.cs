using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ColorPanel : Rect
{
    private HsvSelector HsvSelector = new HsvSelector();
    
    public ColorPanel()
    {
        Default((ref RectSettings settings) => settings = new RectSettings()
        {
            Height = Ui.Grow,
            Color = TextureEditor.PanelColor,
            Direction = UiDirection.TopToBottom,
            Padding = (16, 12),
            BorderRadius = 8,
            Gap = 12,
        });

        Add(new Text(TextureEditor.Font)
            .Default(() => new Text.TextSettings
            {
                Text = "Palette",
                Color = TextureEditor.FontColor,
                Size = 12,
            })
        );

        Rect colorSelector = new Rect()
            .Default((ref RectSettings settings) => settings = new RectSettings
            {
                Direction = UiDirection.TopToBottom,
                Gap = 8,
            });

        int rows = 3;
        int columns = 7;
        
        for (int y = 0; y < rows; y++)
        {
            var rect = new Rect()
                .Default((ref RectSettings settings) => settings = new RectSettings()
                {
                    Gap = 8
                });
            
            for (int x = 0; x < columns; x++)
            {
                //OklabColor color = OklabColor.FromLCH(0.85f, 0.085f, (float)(x + y * columns) / (rows * columns + columns) * Single.Pi * 3);
                HSVColor color = new HSVColor((float)(x + y * columns) / ((rows - 1) * columns + columns), 0.66f, 1);
                
                
                rect.Add(ColorToggle(color));
            }

            colorSelector.Add(rect);
        }
        
        var finalRect = new Rect()
            .Default((ref RectSettings settings) => settings = new RectSettings()
            {
                Gap = 8,
            });

        finalRect.Add(ColorToggle(new Color(1, 1, 1, 1)));
        finalRect.Add(ColorToggle(new Color(0, 0, 0, 1)));

        colorSelector.Add(finalRect);

        Add(colorSelector);
        
        Add(HsvSelector);
        
        // Temp Min Max Toggle
        Add(new Rect()
            .Default((ref RectSettings settings) =>
            {
                settings.Width = Ui.Grow;
                settings.Direction = UiDirection.LeftToRight;
                settings.Alignment = new UiAlignment
                {
                    Vertical = UiAlignmentVertical.Center,
                };
            })
            .Add(new Text(TextureEditor.Font)
                .Default(() => new Text.TextSettings
                {
                    Text = "Crt Min Max",
                    Color = TextureEditor.FontColor,
                    Size = 12,
                })
            )
            .Add(new Spacer())
            .Add(new Toggle()
                .OnValueChanged(value =>
                {
                    TextureEditor.SharedState.UseMinMax = value;
                })
            )
        );
    }

    public Rect ColorToggle(HSVColor color) =>
        new Rect()
            .Default((ref RectSettings settings) =>
            {
                settings.Width = Ui.Pixels(20);
                settings.Height = Ui.Pixels(20);
                settings.Color = color.ToRgb();
                settings.BorderRadius = 10;
                settings.BorderColor = new Color(1, 1, 1, 1);
                settings.BorderSize = TextureEditor.SharedState.SelectedColor == color ? 4 : 0;
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
                TextureEditor.SharedState.SelectColor(color);
                TextureEditor.SharedState.SelectedTool = 
                    TextureEditor.SharedState.Tools[typeof(PenTool)];
            });
}