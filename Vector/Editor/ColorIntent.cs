using WaffleEngine;

namespace Vector.Editor;

// Slider intent for the fill picker, decoupled from the model color.
//
// sRGB can't hold every Oklch pick: ToRGB clamps out-of-gamut choices, so
// reading slider positions back from the model would yank them to "what's
// allowed" while dragging. Instead the sliders own their positions and only
// reseed from the model on shape switch or external change (undo/load) —
// detected by comparing against the last color this intent wrote.
public sealed class ColorIntent
{
    public const float MaxChroma = 0.4f;

    // Slider space: Hue/360, Lightness, Chroma/MaxChroma.
    public float H;
    public float L = 0.65f;
    public float C = 0.25f / MaxChroma;

    private int _shapeId = -1;
    private Color _written;
    private bool _has;

    public OklchColor ToOklch() => new(L, C * MaxChroma, H * 360f);

    public void SyncTo(Shape? shape)
    {
        if (shape is null)
            return;
        if (_has && shape.Id == _shapeId && shape.Color == _written)
            return;
        var oklch = new OklchColor(shape.Color.ToLinear());
        H = NormHue(oklch.Hue) / 360f;
        L = Math.Clamp(oklch.Lightness, 0f, 1f);
        C = Math.Clamp(oklch.Chroma / MaxChroma, 0f, 1f);
        _shapeId = shape.Id;
        _written = shape.Color;
        _has = true;
    }

    public void Wrote(Shape shape, Color color)
    {
        shape.Color = color;
        _shapeId = shape.Id;
        _written = color;
        _has = true;
    }

    private static float NormHue(float degrees) => ((degrees % 360f) + 360f) % 360f;
}
