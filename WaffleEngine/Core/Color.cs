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

    /// <summary>
    /// Generates a color from a HSV color.
    /// </summary>
    /// <param name="hue">0 to 1 for the hue of the color</param>
    /// <param name="saturation"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Color FromHSV(float hue, float saturation, float value)
    {
        Color output = new Color(0, 0, 0, 1);

        // Wrap the hue value.
        hue = (hue % 1 + 1) % 1;
        
        // Get the remainder of 60 degree sections.
        var section = (int)MathF.Floor(hue * 6);
        var remainder = hue * 6f % 1f;

        var full = value;
        var washout = value * (1f - saturation);
        var blendFrom = value * (1f - (saturation * remainder));
        var blendTo = value * (1f - (saturation * (1f - remainder)));

        switch (section)
        {
            // Red -> Yellow
            case 0:
                output.r = full;
                output.g = blendTo;
                output.b = washout;
                break;
            // Yellow -> Green
            case 1:
                output.r = blendFrom;
                output.g = full;
                output.b = washout;
                break;
            // Green -> Cyan
            case 2:
                output.r = washout;
                output.g = full;
                output.b = blendTo;
                break;
            // Cyan -> Blue
            case 3:
                output.r = washout;
                output.g = blendFrom;
                output.b = full;
                break;
            // Blue -> Pink
            case 4:
                output.r = blendTo;
                output.g = washout;
                output.b = full;
                break;
            // Pink -> Red
            default:
                output.r = full;
                output.g = washout;
                output.b = blendFrom;
                break;
        }
        
        return output;
    }

    public static Color White => new Color(1, 1, 1, 1);
    public static Color Black => new Color(0, 0, 0, 1);
    public static Color Transparent => new Color(0, 0, 0, 0);
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

public struct HSVColor : IEquatable<HSVColor>
{
    public float H;
    public float S;
    public float V;

    public float Hue => H;
    public float Saturation => S;
    public float Value => V;

    public Color ToRgb() => Color.FromHSV(H, S, V);

    public HSVColor(float hue, float saturation, float value)
    {
        H = hue;
        S = saturation;
        V = value;
    }

    public static HSVColor FromColor(Color color)
    {
        double red = color.r;
        double green = color.g;
        double blue = color.b;

        // The value is just the maximum value of red, green or blue
        double value = Math.Max(red, Math.Max(green, blue));

        if (!(value > 0))
        {
            return new HSVColor()
            {
                H = 0,
                S = 0,
                V = 0,
            };
        }
        
        // The inverse minimum of red, green or blue scaled to 100% saturation.
        double minimum = Math.Min(red, Math.Min(green, blue));
        double saturation = 1f - (minimum / value);

        // If saturation is 0 its impossible to know what hue it is so just return 0.
        if (saturation <= 0)
        {
            return new HSVColor
            {
                H = 0,
                S = (float)saturation,
                V = (float)value,
            };
        }
        
        double hue;

        double delta = value - minimum;
        
        // In either segment 0 or 5
        if (red >= value)
        {
            hue = (green - blue) / delta;
        }
        // In either segment 1 or 2
        else if (green >= value)
        {
            hue = 2.0 + (blue - red) / delta;
        }
        // In either segment 3 or 4
        else
        {
            hue = 4.0 + (red - green) / delta;
        }

        if (hue < 0.0)
            hue += 6.0;

        hue /= 6.0;

        return new HSVColor
        {
            H = (float)hue,
            S = (float)saturation,
            V = (float)value,
        };
    }

    public static implicit operator Color(HSVColor color) => Color.FromHSV(color.H, color.S, color.V);
    public static implicit operator HSVColor(Color color) => FromColor(color);
    
    public static bool operator ==(HSVColor left, HSVColor right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(HSVColor left, HSVColor right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(HSVColor other)
    {
        return H == other.H && S == other.S && V == other.V;
    }

    public override bool Equals(object? obj)
    {
        return obj is HSVColor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(H, S, V);
    }
}