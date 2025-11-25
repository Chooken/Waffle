using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory;

public struct UICrtData
{
    public AlignedVector3 Position;
    public Vector2 Size;
    public Vector4 TopLeftColor;
    public Vector4 TopRightColor;
    public Vector4 BottomLeftColor;
    public Vector4 BottomRightColor;
    public Vector4 BorderRadius;
    public Vector4 BorderColor;
    public Vector2 ScreenSize;
    public Vector2 RefRes;
    public float ChromaticAberration;
    public float BorderSize;
    public int UseMinMax;
}

public class RectCrt(GpuTexture texture, float chromaticAberration) : Rect
{
    public float ChromaticAberration = chromaticAberration;
    public GpuTexture Texture = texture;
    public bool UseMinMax = false;

    private static Shader? _shader;

    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        if (_shader is null)
        {
            if (!SetupCrtShader())
                return;
        }

        UICrtData data = new UICrtData()
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
            RefRes = new Vector2(Texture.Width, Texture.Height),
            ChromaticAberration = ChromaticAberration,
            BorderSize = RectSettings.BorderSize,
            UseMinMax = UseMinMax ? 1 : 0,
        };

        renderPass.SetUniforms(data);

        renderPass.Bind(_shader);
        renderPass.Bind(Texture);

        renderPass.DrawPrimatives(6, 1, 0, 0);
    }

    private bool SetupCrtShader()
    {
        if (!Assets.TryGetShader("Core", "crt", out _shader))
        {
            //Log.Error("Shader not found");
            return false;
        }

        return true;
    }
}