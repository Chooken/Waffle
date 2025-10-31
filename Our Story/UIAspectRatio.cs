using WaffleEngine;
using WaffleEngine.UI;

namespace OurStory;

public class UIAspectRatio : UIRect
{
    public Vector2 AspectRatio;
    public Vector2 PixelMultiple;
    
    public override Vector2 GetSize(Vector2 parentSize)
    {
        bool widthBound = parentSize.x < parentSize.y;

        Vector2 shrunkSize;
        Vector2 scaling = new Vector2(1, 1);

        scaling.x = PixelMultiple.x != 0 ? PixelMultiple.x : 1;
        scaling.y = PixelMultiple.y != 0 ? PixelMultiple.y : 1;

        if (widthBound)
        {
            float width = Settings.Width.AsPixels(parentSize);

            float scaledWidth = MathF.Floor(width / scaling.x) * scaling.x;
            float scaledHeight = MathF.Floor(width / AspectRatio.x * AspectRatio.y / scaling.x) * scaling.x;
            
            shrunkSize = new Vector2(
                scaledWidth != 0 ? scaledWidth : width, 
                scaledHeight != 0 ? scaledHeight : width / AspectRatio.x * AspectRatio.y);
        }
        else
        {
            float height = Settings.Height.AsPixels(parentSize);

            float scaledWidth = MathF.Floor(height / AspectRatio.y * AspectRatio.x / scaling.x) * scaling.x;
            float scaledHeight = MathF.Floor(height / scaling.x) * scaling.x;
            
            shrunkSize = new Vector2(
                scaledWidth != 0 ? scaledWidth : height / AspectRatio.y * AspectRatio.x,
                scaledHeight != 0 ? scaledHeight : height);
        }
        
        return shrunkSize;
    }

    public UIAspectRatio SetAspectRatio(Vector2 aspectRatio)
    {
        AspectRatio = aspectRatio;
        SetDirty();
        return this;
    }

    public UIAspectRatio SetPixelMultiple(Vector2 referenceSize)
    {
        PixelMultiple = referenceSize;
        SetDirty();
        return this;
    }
}