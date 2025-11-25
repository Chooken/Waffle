namespace WaffleEngine.UI;

public class VerticalSeperator : Rect
{
    public VerticalSeperator(Color color)
    {
        Default((ref RectSettings settings) =>
        {
            settings.Height = Ui.Grow;
            settings.Padding = (4, 0);
            settings.Alignment = new UiAlignment
            {
                Horizontal = UiAlignmentHorizontal.Center,
            };
        });

        Add(new Rect()
            .Default((ref RectSettings settings) =>
            {
                settings.Width = Ui.Pixels(2);
                settings.Height = Ui.Grow;
                settings.Color = color;
            })
        );
    }
}

public class HorizontalSeperator : Rect
{
    public HorizontalSeperator(Color color)
    {
        Default((ref RectSettings settings) =>
        {
            settings.Width = Ui.Grow;
            settings.Padding = (4, 0);
            settings.Alignment = new UiAlignment
            {
                Horizontal = UiAlignmentHorizontal.Center,
            };
        });

        Add(new Rect()
            .Default((ref RectSettings settings) =>
            {
                settings.Width = Ui.Grow;
                settings.Height = Ui.Pixels(2);
                settings.Color = color;
            })
        );
    }
}