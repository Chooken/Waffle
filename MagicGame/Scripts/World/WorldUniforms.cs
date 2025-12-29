using System.Numerics;
using System.Runtime.InteropServices;

namespace MagicGame.Scripts;

[StructLayout(LayoutKind.Sequential)]
public struct WorldUniforms
{
    public Matrix4x4 ProjectionMatrix;
}