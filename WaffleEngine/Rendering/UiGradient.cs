using System.Runtime.InteropServices;

namespace WaffleEngine.Rendering;

// Uniforms for the builtin ui-gradient shader (color slider tracks).
// Float4-only so the GPU layout matches byte-for-byte (7 x 16 = 112 bytes).
// Modes: 0 = hue ring (H = t*360, L/C from params), 1 = lightness ramp
// (L = t), 2 = chroma ramp (C = t*0.4). Params always carries the current
// (L, C, H degrees); each mode overrides one channel.
[StructLayout(LayoutKind.Sequential)]
public struct UiGradientData
{
    public Vector4 Position;
    public Vector4 Size;
    public Vector4 RenderSize;
    public Vector4 Clip;
    public Vector4 Mode;
    public Vector4 Params;
    public Vector4 Pad;

    public static UiGradientData Ramp(Vector2 position, Vector2 size, Vector2 renderSize,
        Vector2 clipMin, Vector2 clipMax, int mode, float lightness, float chroma, float hueDeg)
    {
        return new UiGradientData
        {
            Position = new Vector4(position.x, position.y, 0, 0),
            Size = new Vector4(size.x, size.y, 0, 0),
            RenderSize = new Vector4(renderSize.x, renderSize.y, 0, 0),
            Clip = new Vector4(clipMin.x, clipMin.y, clipMax.x, clipMax.y),
            Mode = new Vector4(mode, 0, 0, 0),
            Params = new Vector4(lightness, chroma, hueDeg, 0),
            Pad = Vector4.Zero,
        };
    }
}
