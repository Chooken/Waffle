namespace WaffleEngine.UI;

public class Spacer : Rect
{
    public Spacer()
    {
        Default((ref RectSettings settings) =>
        {
            settings.Width = Ui.Grow;
            settings.Height = Ui.Grow;
        });
    }
}