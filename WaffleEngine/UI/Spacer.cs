namespace WaffleEngine.UI;

public class Spacer : Rect
{
    public Spacer()
    {
        Default(() => new RectSettings
        {
            Width = Ui.Grow,
            Height = Ui.Grow,
        });
    }
}