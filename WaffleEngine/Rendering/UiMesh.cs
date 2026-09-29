using System.Runtime.InteropServices;

namespace WaffleEngine.Rendering;

// Uniforms for the builtin ui-mesh shader. Float4-only fields so the GPU
// layout matches byte-for-byte (4 x 16 = 64 bytes, no HLSL packing gaps).
// Pushed via ImRenderPass.SetUniforms, which uploads the same bytes to the
// vertex (space1) and fragment (space3) uniform slots.
[StructLayout(LayoutKind.Sequential)]
public struct UiMeshData
{
    public Vector4 Offset;      // xy = screen-space pixel offset
    public Vector4 RenderSize;  // xy = render target size in pixels
    public Vector4 Tint;
    public Vector4 Clip;        // xy = clip min, zw = clip max (pixels)

    public static UiMeshData Text(
        Vector2 position, Vector2 renderSize, Vector2 clipMin, Vector2 clipMax)
    {
        return new UiMeshData
        {
            Offset = new Vector4(position.x, position.y, 0, 0),
            RenderSize = new Vector4(renderSize.x, renderSize.y, 0, 0),
            Tint = Vector4.One,
            Clip = new Vector4(clipMin.x, clipMin.y, clipMax.x, clipMax.y),
        };
    }
}
