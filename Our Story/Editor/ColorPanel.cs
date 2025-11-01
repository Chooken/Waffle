using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class ColorPanel
{
    public Color SelectedColor { get; private set; }
    public UIRect Panel;
    public Action<Color> OnColorSelected;

    public static UIRect Create()
    {
        ColorPanel colorPanel = new ColorPanel();
        return colorPanel.Panel;
    }

    public ColorPanel()
    {
        Panel = new UIRect()
            .Default(() => new UISettings()
            {
                Color = Color.RGBA255(22, 22, 22, 255),
                ChildDirection = UIDirection.Down,
                PaddingX = UISize.Pixels(20),
                PaddingY = UISize.Pixels(20),
                BorderRadius = new UIBorderRadius(20, 20, 20, 20, UISizeType.Pixels)
            });

        int rows = 17;
        int columns = 7;
        
        for (int y = 0; y < rows - 1; y++)
        {
            var rect = new UIRect()
                .Default(() => new UISettings()
                {
                    Gap = UISize.Pixels(4),
                    PaddingY = UISize.Pixels(2),
                });
            
            for (int x = 0; x < columns; x++)
            {
                OklabColor color = OklabColor.FromLCH((1f - (float)x / columns) * 0.7f + 0.3f, (1f - (float)x / columns) * 0.125f, (float)y / rows * Single.Pi * 2);
                
                rect.AddUIElement(ColorToggle(color));
            }

            Panel.AddUIElement(rect);
        }
        
        var finalRect = new UIRect()
            .Default(() => new UISettings()
            {
                Gap = UISize.Pixels(4),
                PaddingY = UISize.Pixels(2),
            });

        finalRect.AddUIElement(ColorToggle(new Color(1, 1, 1, 1)));
        finalRect.AddUIElement(ColorToggle(new Color(0, 0, 0, 1)));
        finalRect.AddUIElement(ColorToggle(new Color(0, 0, 0, 0)));

        Panel.AddUIElement(finalRect);
    }
    
    public UIRect ColorToggle(Color color) =>
        new UIRect()
            .Default(() => new UISettings()
            {
                Width = UISize.Pixels(18),
                Height = UISize.Pixels(18),
                Color = color.WithAlphaOne(),
                BorderColor = new Color(1,1,1,1),
                BorderSize = UISize.Pixels(SelectedColor == color ? 4 : 0),
            })
            .OnHover((ref UISettings settings) =>
            {
                if (SelectedColor == color)
                    return;
                
                settings.BorderColor = new Color(0, 0, 0, 1);
                settings.BorderSize = UISize.Pixels(4);
            })
            .OnClick((ref UISettings settings) =>
            {
                SelectedColor = color;
                OnColorSelected?.Invoke(color);
            });

    public static implicit operator UIRect(ColorPanel panel) => panel.Panel;
}