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

    public static implicit operator Vector4(Color color) => new Vector4(color.r, color.g, color.b, color.a);
    public static implicit operator SDL.Color(Color color) => new SDL.Color() { R = color.r255, G = color.g255, B = color.b255, A = color.a255 };

    public static Color RGBA255(uint r, uint g, uint b, uint a) =>
        new Color((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255);
}