using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class Toggle : Rect
{
    private bool _value;
    private Action<bool> _onValueChanged;
    
    public Toggle()
    {
        Default(() => new RectSettings
        {
            Width = Ui.Pixels(45),
            Color = TextureEditor.BackgroundColor,
            Padding = 4,
            BorderRadius = 25,
            Alignment = new UiAlignment()
            {
                Horizontal = _value ? UiAlignmentHorizontal.Right : UiAlignmentHorizontal.Left,
            }
        });

        Add(new Rect()
            .Default(() => new RectSettings
            {
                Width = Ui.Pixels(20),
                Height = Ui.Pixels(20),
                BorderRadius = 10,
                Color = _value ? TextureEditor.Highlight : TextureEditor.PanelColor,
            })
        );

        OnHover((ref RectSettings settings) =>
        {
            settings.Cursor = Cursor.Pointer;
        });

        OnMouseDown((ref RectSettings settings) =>
        {
            _value = !_value;
            _onValueChanged?.Invoke(_value);
        });
    }

    public void SetValue(bool value) => _value = value;

    public Toggle OnValueChanged(Action<bool> action)
    {
        _onValueChanged += action;
        return this;
    }
}