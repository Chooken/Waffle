using System.Numerics;

namespace WaffleEngine;

public struct Color(float red, float green, float blue, float alpha = 1.0f)
{
    public float r = red;
    public float g = green;
    public float b = blue;
    public float a = alpha;
    
    public float Red { get => r; set => r = value;  }
    public float Green { get => g; set => g = value; }
    public float Blue { get => b; set => b = value;  }
    public float Alpha { get => a; set => a = value;  }
    
    public float x { get => r; set => r = value;  }
    public float y { get => g; set => g = value;  }
    public float z { get => b; set => b = value;  }
    public float w { get => a; set => a = value;  }

    public static implicit operator Vector4(Color color) => new Vector4(color.r, color.g, color.b, color.w);
}