using SDL3;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;

namespace WaffleEngine.UI;

public class UIText : UIRect
{
    public Color TextColor;
    private string? _text;
    private Texture _texture;

    public UIText()
    {
        FontLoader.LoadFont("Fonts/Nunito-Regular.ttf", 28);
    }

    public override Vector2 Render(ImQueue queue, ImRenderPass renderPass, Vector3 position, UIAnchor anchor, Vector2 parentSize,
        Vector2 renderSize)
    {
        if (_text is not null)
            RenderText(parentSize);
        
        return base.Render(queue, renderPass, position, anchor, parentSize, renderSize);
    }

    private void RenderText(Vector2 parentSize)
    {
        Color = Parent?.Color ?? Color.RGBA255(0, 0, 0, 255);
        
        if (!FontLoader.TryRenderTextToTexture(_text!, "Fonts/Nunito-Regular.ttf", TextColor,
                Color, (int)parentSize.x - (int)MarginX.AsPixels(parentSize.x) * 2, ref _texture))
            return;
        
        ImQueue queue = new ImQueue();
        var copyPass = queue.AddCopyPass();
        copyPass.Upload(_texture);
        copyPass.End();
        queue.Submit();

        Texture = _texture;
        
        Width = UISize.Pixels(_texture.Width + (int)MarginX.AsPixels(parentSize.x) * 2);
        Height = UISize.Pixels(_texture.Height + (int)MarginY.AsPixels(parentSize.x) * 2);
    }

    public UIText SetText(string text)
    {
        _text = text;
        SetDirty();
        return this;
    }

    public UIText SetTextColor(Color color)
    {
        TextColor = color;
        SetDirty();
        return this;
    }
}