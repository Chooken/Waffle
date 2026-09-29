using WaffleEngine;

namespace Vector.Editor;

// Single owner of pan/zoom + world<->screen math.
// World space: -1..1 asset space (matches VectorRenderer).
// Screen space: pixel coordinates from Input.Mouse.Position / INode.Rect.
public sealed class ViewportCamera
{
    public Vector2 Pan;
    public float Zoom = 1f;

    public const float MinZoom = 0.25f;
    public const float MaxZoom = 16f;

    public void ApplyZoom(float wheelDelta, Vector2 cursorScreen, Vector2 viewportCenter, float baseSize)
    {
        if (wheelDelta == 0)
            return;
        float factor = MathF.Pow(1.15f, MathF.Sign(wheelDelta));
        float next = Math.Clamp(Zoom * factor, MinZoom, MaxZoom);
        if (next == Zoom)
            return;

        // Keep the world point under the cursor stable.
        Vector2 before = ScreenToWorld(cursorScreen, viewportCenter, baseSize);
        Zoom = next;
        Vector2 after = ScreenToWorld(cursorScreen, viewportCenter, baseSize);
        float half = baseSize * Zoom / 2f;
        Pan = new Vector2(
            Pan.x + (after.x - before.x) * half,
            Pan.y + (before.y - after.y) * half);
    }

    public Vector2 ScreenToWorld(Vector2 screen, Vector2 viewportCenter, float baseSize)
    {
        float half = baseSize * Zoom / 2f;
        if (half <= 0f)
            return Vector2.Zero;
        return new Vector2(
            (screen.x - viewportCenter.x - Pan.x) / half,
            -(screen.y - viewportCenter.y - Pan.y) / half);
    }

    public Vector2 WorldToScreen(Vector2 world, Vector2 viewportCenter, float baseSize)
    {
        float half = baseSize * Zoom / 2f;
        return new Vector2(
            viewportCenter.x + Pan.x + world.x * half,
            viewportCenter.y + Pan.y - world.y * half);
    }

    public float PixelsPerWorldUnit(float baseSize) => baseSize * Zoom / 2f;

    public Vector2 SnapToPixel(Vector2 world, float baseSize, int canvasResolution)
    {
        if (canvasResolution <= 0)
            return world;
        float step = 2f / canvasResolution;
        return new Vector2(
            MathF.Round(world.x / step) * step,
            MathF.Round(world.y / step) * step);
    }
}
