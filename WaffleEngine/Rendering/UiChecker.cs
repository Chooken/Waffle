using System.Runtime.InteropServices;

namespace WaffleEngine.Rendering;

// Uniforms for the builtin ui-checker shader (canvas transparency grid).
// Float4-only so the GPU layout matches byte-for-byte (7 x 16 = 112 bytes).
[StructLayout(LayoutKind.Sequential)]
public struct UiCheckerData
{
    public Vector4 Position;
    public Vector4 Size;
    public Vector4 RenderSize;
    public Vector4 Clip;
    public Vector4 Grid;
    public Vector4 ColorA;
    public Vector4 ColorB;

    public static UiCheckerData GridQuad(Vector2 position, Vector2 size, Vector2 renderSize,
        Vector2 clipMin, Vector2 clipMax, float canvasTexels, float texelsPerSquare,
        Color colorA, Color colorB)
    {
        return new UiCheckerData
        {
            Position = new Vector4(position.x, position.y, 0, 0),
            Size = new Vector4(size.x, size.y, 0, 0),
            RenderSize = new Vector4(renderSize.x, renderSize.y, 0, 0),
            Clip = new Vector4(clipMin.x, clipMin.y, clipMax.x, clipMax.y),
            Grid = new Vector4(canvasTexels, Math.Max(1f, texelsPerSquare), 0, 0),
            ColorA = colorA,
            ColorB = colorB,
        };
    }
}
