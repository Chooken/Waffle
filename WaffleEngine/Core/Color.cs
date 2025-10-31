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
    
    public float Red { get => r; set => r = value;  }
    public float Green { get => g; set => g = value; }
    public float Blue { get => b; set => b = value;  }
    public float Alpha { get => a; set => a = value;  }
    
    public float x { get => r; set => r = value;  }
    public float y { get => g; set => g = value;  }
    public float z { get => b; set => b = value;  }
    public float w { get => a; set => a = value;  }

    public Color WithAlphaOne() => new Color(r, g, b, 1);

    public static implicit operator Vector4(Color color) => new Vector4(color.r, color.g, color.b, color.a);
    public static implicit operator SDL.Color(Color color) => new SDL.Color() { R = color.r255, G = color.g255, B = color.b255, A = color.a255 };

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

    public Color GammaCorrected() => new Color(
        Gamma(r),
        Gamma(g),
        Gamma(b),
        a);
    
    private float Gamma(float x)
    {
        return x <= 0.0031308f
            ? x * 12.92f
            : MathF.Pow(x, 1.0f/2.4f) * 1.055f - 0.055f;
    }
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
    
    public static OklabColor FromLCH(float lightness, float chroma, float hue) => new OklabColor(
        lightness,
        chroma * MathF.Cos(hue),
        chroma * MathF.Sin(hue)
        );

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

    public static implicit operator Color(OklabColor color) => color.ToRGB();
}