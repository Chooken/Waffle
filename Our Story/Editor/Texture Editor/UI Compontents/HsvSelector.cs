using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class HsvSelector : Rect
{
    private HSVColor _color = new HSVColor
    {
        H = 0,
        S = 1,
        V = 1,
    };

    private Texture _hueSliderTexture = new Texture(6, 1);

    public ImageSlider Hue;
    public Slider Saturation;
    public Slider Value;
    
    public HsvSelector()
    {
        Hue = new ImageSlider(TextureEditor.PanelColor)
            .OnValueChanged(value =>
            {
                _color.H = value;
                TextureEditor.SharedState.SelectColor(_color);
            });
        
        Hue.Trench.Default((ref RectSettings settings) =>
        {
            var colors = _hueSliderTexture.GetAs<(byte r, byte g, byte b, byte a)>();

            for (int i = 0; i < colors.Length; i++)
            {
                var colorHsv = _color with { H = (float)i / (colors.Length - 1) };
                var color = colorHsv.ToRgb();
                colors[i] = (color.r255, color.g255, color.b255, color.a255);
            }

            settings.Height = Ui.Pixels(8);
            settings.BorderRadius = 4;
        });

        Hue.Trench.Texture = _hueSliderTexture;
        
        
        Saturation = new Slider(TextureEditor.PanelColor)
            .OnValueChanged(value =>
            {
                _color.S = value;
                TextureEditor.SharedState.SelectColor(_color);
            });
        
        Saturation.Trench.Default((ref RectSettings settings) =>
        {
            var leftColor = _color with { S = 0 };
            var rightColor = _color with { S = 1};
            
            settings.Color = (leftColor, rightColor);
            settings.Height = Ui.Pixels(8);
            settings.BorderRadius = 4;
        });
        
        Value = new Slider(TextureEditor.PanelColor)
            .OnValueChanged(value =>
            {
                _color.V = value;
                TextureEditor.SharedState.SelectColor(_color);
            });
        
        Value.Trench.Default((ref RectSettings settings) =>
        {
            var leftColor = _color with { V = 0 };
            var rightColor = _color with { V = 1};
            
            settings.Color = (leftColor, rightColor);
            settings.Height = Ui.Pixels(8);
            settings.BorderRadius = 4;
        });
        
        Default((ref RectSettings settings) =>
        {
            settings.Width = Ui.Grow;
            settings.Direction = UiDirection.TopToBottom;
        });

        Add(Hue);
        Add(Saturation);
        Add(Value);
    }

    public override void Update()
    {
        _color = TextureEditor.SharedState.SelectedColor;
        Hue.Value = _color.H;
        Hue.NobColor = _color;
        Saturation.Value = _color.S;
        Saturation.NobColor = _color;
        Value.Value = _color.V;
        Value.NobColor = _color;
        base.Update();
    }
}