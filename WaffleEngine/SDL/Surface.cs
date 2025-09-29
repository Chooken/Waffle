using System.Runtime.InteropServices;
using SDL3;

namespace WaffleEngine.SDLExtra;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Surface
{
    public readonly SDL.SurfaceFlags Flags;
    public readonly SDL.PixelFormat Format;
    public readonly int Width;
    public readonly int Height;
    public readonly int Pitch;
    private void* _pixels;
    private int _refCount;
    private IntPtr unused;

    public Span<byte> Pixels => new Span<byte>(_pixels, Width * Height * 4);
}