using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ColorPanel : Rect
{
    private Slider BrightnessSlider = new Slider(TextureEditor.PanelColor)
        .OnValueChanged(value =>
        {
            TextureEditor.SharedState.ColorBrightness = value;
        });
    
    public ColorPanel()
    {
        Default(() => new RectSettings()
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
        
        // Brightness Slider
        Add(new Text(TextureEditor.Font)
            .Default(() => new Text.TextSettings
            {
                Text = "Brightness",
                Color = TextureEditor.FontColor,
                Size = 12,
            })
        );
        
        Add(BrightnessSlider);
        
        // Temp Min Max Toggle
        Add(new Rect()
            .Default(() => new RectSettings
            {
                Width = Ui.Grow,
                Direction = UiDirection.LeftToRight,
                Alignment = new UiAlignment
                {
                    Vertical = UiAlignmentVertical.Center,
                },
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

    public override void Update()
    {
        Color color = TextureEditor.SharedState.SelectedColor;
        float brightness = TextureEditor.SharedState.ColorBrightness;

        color.r *= brightness;
        color.g *= brightness;
        color.b *= brightness;

        BrightnessSlider.NobColor = color;
        BrightnessSlider.Value = TextureEditor.SharedState.ColorBrightness;
        base.Update();
    }

    public Rect ColorToggle(Color color) =>
        new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Pixels(20),
                Height = Ui.Pixels(20),
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