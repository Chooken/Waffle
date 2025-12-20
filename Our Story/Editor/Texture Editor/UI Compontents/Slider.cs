using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class Slider : Rect
{
    public float Value;

    public Color NobColor;

    private float NobSize = 24;
    private Action<float> _onValueChanged;

    public Rect Trench;
    public Rect Nob;
    
    public Slider(Color nob)
    {
        NobColor = nob;
        
        Trench = CreateTrench();
        
        Default((ref RectSettings settings) =>
        {
            settings.Height = Ui.Pixels(24);
            settings.Width = Ui.Grow;
            settings.Alignment = new UiAlignment
            {
                Vertical = UiAlignmentVertical.Center,
                Horizontal = UiAlignmentHorizontal.Center,
            };
        });

        OnHover((ref RectSettings settings) =>
        {
            settings.Cursor = Cursor.Pointer;
        });

        OnMouseDown((ref RectSettings settings) =>
        {
            NobSize = 28;
        });

        OnHold((ref RectSettings settings) =>
        {
            var pos = Trench.RelativePosition(Input.Mouse.Position);
            Value = float.Clamp(pos.x, 0, 1);
            _onValueChanged?.Invoke(Value);
        });

        OnMouseUp((ref RectSettings settings) =>
        {
            NobSize = 24;
        });

        Add(Trench
            .Add(CreateNob())
        );
    }
    
    public Slider OnValueChanged(Action<float> action)
    {
        _onValueChanged += action;
        return this;
    }

    private Rect CreateNob() => new Rect()
        .Default((ref RectSettings settings) =>
        {
            settings.Position = Ui.Relative
                .Left((Trench.Bounds.CalculatedWidth - Trench.Settings.Padding.TotalHorizontal) * Value -
                      NobSize * 0.5f)
                .Top(Trench.Bounds.CalculatedHeight * 0.5f - NobSize * 0.5f);
            settings.Width = Ui.Pixels(NobSize);
            settings.Height = Ui.Pixels(NobSize);
            settings.BorderRadius = NobSize * 0.5f;
            settings.Color = NobColor;
            settings.BorderColor = TextureEditor.BackgroundColor;
            settings.BorderSize = 4;
        });

    private Rect CreateTrench() => new Rect()
        .Default((ref RectSettings settings) =>
        {
            settings.Width = Ui.Grow;
            settings.Height = Ui.Pixels(4);
            settings.BorderRadius = 2;
            settings.Color = TextureEditor.BackgroundColor;
            settings.Padding = (12, 0);
        });
}