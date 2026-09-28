using System.Numerics;
using SDL3;

namespace WaffleEngine;

public struct Color(float red, float green, float blue, float alpha = 1.0f)
{
    public float r = red;
    public float g = green;
    public float b = blue;
    public float a = alpha;

    public byte r255 => (byte)(r * 255);
    public byte g255 => (byte)(g * 255);
    public byte b255 => (byte)(b * 255);
    public byte a255 => (byte)(a * 255);
    
    public float x { get => r; set => r = value;  }
    public float y { get => g; set => g = value;  }
    public float z { get => b; set => b = value;  }
    public float w { get => a; set => a = value;  }

    public Color WithAlphaOne() => new Color(r, g, b, 1);

    public static implicit operator Vector4(Color color) => new Vector4(color.r, color.g, color.b, color.a);
    public static implicit operator SDL.Color(Color color) => new SDL.Color() { R = color.r255, G = color.g255, B = color.b255, A = color.a255 };

    public static Color RGB(float r, float g, float b) =>
        new Color(r, g, b, 1);
    public static Color RGBA(float r, float g, float b, float a) =>
        new Color(r, g, b, a);
    public static Color RGBA255(uint r, uint g, uint b, uint a) =>
        new Color((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255);

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(Color other)
    {
        return r == other.r && g == other.g && b == other.b && a == other.a;
    }

    public Color ToGamma() => new Color(
        LinearToGamma(r),
        LinearToGamma(g),
        LinearToGamma(b),
        a);

    public Color ToLinear() => new Color(
        GammaToLinear(r),
        GammaToLinear(g),
        GammaToLinear(b),
        a);
    
    private float LinearToGamma(float x)
    {
        return x <= 0.0031308f
            ? x * 12.92f
            : MathF.Pow(x, 1.0f/2.4f) * 1.055f - 0.055f;
    }
    
    private static float GammaToLinear(float c)
    {
        return c <= 0.04045f
            ? c / 12.92f
            : (float)Math.Pow((c + 0.055f) / 1.055f, 2.4);
    }
    
    public static Color White => new OklchColor(1, 0, 0).ToRGB();
    public static Color Gray => new OklchColor(0.5f, 0, 0).ToRGB();
    public static Color Black => new OklchColor(0, 0, 0).ToRGB();
    public static Color HotPink => new OklchColor(0.65f, 0.25f, 0f).ToRGB();
    public static Color BrightRed => new OklchColor(0.65f, 0.25f, 30f).ToRGB();
    public static Color Orange =>  new OklchColor(0.65f, 0.25f, 60f).ToRGB();
    public static Color Yellow =>  new OklchColor(0.65f, 0.25f, 90f).ToRGB();
    public static Color Lime =>  new OklchColor(0.65f, 0.25f, 120f).ToRGB();
    public static Color Green =>  new OklchColor(0.65f, 0.25f, 150f).ToRGB();
    public static Color Turquoise =>  new OklchColor(0.65f, 0.25f, 180f).ToRGB();
    public static Color Aquamarine =>  new OklchColor(0.65f, 0.25f, 210f).ToRGB();
    public static Color Azure =>  new OklchColor(0.65f, 0.25f, 240f).ToRGB();
    public static Color PastelBlue =>  new OklchColor(0.65f, 0.25f, 270f).ToRGB();
    public static Color Purple =>  new OklchColor(0.65f, 0.25f, 300f).ToRGB();
    public static Color Pink => new OklchColor(0.65f, 0.25f, 0f).ToRGB();
}

public struct OklabColor
{
    public float Lightness;
    public float GreenRed;
    public float BlueYellow;

    public float L => Lightness;
    public float A => GreenRed;
    public float B => BlueYellow;

    public Color Color => ToRGB();

    public OklabColor(float lightness, float greenRed, float blueYellow)
    {
        Lightness = lightness;
        GreenRed = greenRed;
        BlueYellow = blueYellow;
    }

    public OklabColor(Color color)
    {
        this = fromRGB(color.r, color.g, color.b);
    }

    public static OklabColor fromRGB(float r, float g, float b)
    {
        float l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
        float m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
        float s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

        float l_ = (float)Math.Cbrt(l);
        float m_ = (float)Math.Cbrt(m);
        float s_ = (float)Math.Cbrt(s);

        float L = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_;
        float A = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_;
        float B = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_;

        return new OklabColor(L, A, B);
    }

    public Color ToRGB()
    {
        float l_ = L + 0.3963377774f * A + 0.2158037573f * B;
        float m_ = L - 0.1055613458f * A - 0.0638541728f * B;
        float s_ = L - 0.0894841775f * A - 1.2914855480f * B;
        
        float l = MathF.Pow(l_, 3);
        float m = MathF.Pow(m_, 3);
        float s = MathF.Pow(s_, 3);

        return new Color(
        float.Clamp(+4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s, 0, 1),
        float.Clamp(-1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s, 0, 1),
        float.Clamp(-0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s, 0, 1)
        );
    }
}

public struct OklchColor
{
    public float Lightness;
    public float Chroma;
    public float Hue;

    public float L => Lightness;
    public float C => Chroma;
    public float H => Hue;

    public Color Color => ToRGB();

    public OklchColor(float lightness, float chroma, float hue)
    {
        Lightness = lightness;
        Chroma = chroma;
        Hue = hue;
    }

    public OklchColor(Color color)
    {
        OklabColor lab = new OklabColor(color);
        Lightness = lab.L;
        Chroma = MathF.Sqrt(lab.A * lab.A + lab.B * lab.B);
        Hue = MathF.Atan2(lab.B, lab.A);
    }

    public OklabColor ToLAB()
    {
        float half_radius = Hue * MathF.PI / 180.0f;
        return new OklabColor(
            Lightness,
            Chroma * MathF.Cos(half_radius),
            Chroma * MathF.Sin(half_radius)
        );
    }

    public Color ToRGB()
    {
        OklabColor lab = ToLAB();
        return lab.ToRGB();
    }
}