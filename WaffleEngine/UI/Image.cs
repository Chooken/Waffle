using System.Net.Mime;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class Image : Rect
{
    public Texture Texture;
    private Shader? _shader;

    public Image(Texture? texture)
    {
        if (texture is null)
        {
            texture = new Texture(2, 2);
            var data = texture.GetAs<(byte r, byte g, byte b, byte a)>();
            data[0] = (1, 1, 1, 1);
        }

        Texture = texture;
    }

    public override void Update()
    {
        var queue = new ImQueue();
        var copypass = queue.AddCopyPass();
        copypass.Upload(Texture);
        copypass.End();
        queue.Submit();
        
        base.Update();
    }

    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        if (_shader is null)
        {
            if (!SetupShader())
            {
                return;
            }
        }
        
        UIRectData data = new UIRectData()
        {
            Position = new AlignedVector3(Bounds.CalculatedPosition),
            Size = new Vector2(Bounds.CalculatedWidth, Bounds.CalculatedHeight),
            TopLeftColor = RectSettings.Color.TopLeft,
            TopRightColor = RectSettings.Color.TopRight,
            BottomLeftColor = RectSettings.Color.BottomLeft,
            BottomRightColor = RectSettings.Color.BottomRight,
            BorderRadius = new Vector4(
                RectSettings.BorderRadius.BottomLeft, 
                RectSettings.BorderRadius.TopLeft, 
                RectSettings.BorderRadius.BottomRight, 
                RectSettings.BorderRadius.TopRight),
            BorderColor = RectSettings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = RectSettings.BorderSize,
        };
        
        renderPass.SetUniforms(data);
        renderPass.Bind(_shader);
        renderPass.Bind(Texture);
        renderPass.DrawPrimatives(6, 1, 0, 0);
    }
    
    private bool SetupShader()
    {
        if (!Assets.TryGetShader("builtin", "ui-image", out _shader))
        {
            Log.Error("Shader not found");
            return false;
        }

        return true;
    }
}