using System.Numerics;
using System.Runtime.InteropServices;

namespace WaffleEngine.Rendering;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex
{
    public Vector4 Color;
    public Vector4 Position;
    public Vector2 Uv;
}