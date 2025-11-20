namespace WaffleEngine.UI;

public class VerticalSeperator : Rect
{
    public VerticalSeperator(Color color)
    {
        Default(() => new RectSettings
        {
            Height = Ui.Grow,
            Padding = (4, 0),
            Alignment = new UiAlignment
            {
                Horizontal = UiAlignmentHorizontal.Center,
            },
        });

        Add(new Rect()
            .Default(() => new RectSettings
            {
                Width = Ui.Fixed(2),
                Height = Ui.Grow,
                Color = color,
            })
        );
    }
}

public class HorizontalSeperator : Rect
{
    public HorizontalSeperator(Color color)
    {
        Default(() => new RectSettings
        {
            Width = Ui.Grow,
            Padding = (4, 0),
            Alignment = new UiAlignment
            {
                Horizontal = UiAlignmentHorizontal.Center,
            },
        });

        Add(new Rect()
            .Default(() => new RectSettings
            {
                Width = Ui.Grow,
                Height = Ui.Fixed(2),
                Color = color,
            })
        );
    }
}