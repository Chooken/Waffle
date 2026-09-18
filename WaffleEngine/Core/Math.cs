using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace WaffleEngine;

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct Vector2
{
    public float x, y;

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2 Zero => new Vector2(0, 0);
    
    
    public static Vector2 One => new Vector2(1, 1);
    
    public static implicit operator System.Numerics.Vector2(Vector2 vector) =>
        new (vector.x, vector.y);
    
    public static bool operator ==(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    
    public static bool operator !=(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y;
    }

    public static Vector2 operator *(Vector2 lhs, int rhs)
    {
        return new Vector2(lhs.x * rhs, lhs.y * rhs);
    }
    
    public static Vector2 operator *(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.x * rhs, lhs.y * rhs);
    }
    
    public static Vector2 operator /(Vector2 lhs, int rhs)
    {
        return new Vector2(lhs.x / rhs, lhs.y / rhs);
    }
    
    public static Vector2 operator /(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.x / rhs, lhs.y / rhs);
    }
    
    public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.x - rhs.x, lhs.y - rhs.y);
    }

    public static Vector2 operator -(Vector2 lhs, int rhs)
    {
        return new Vector2(lhs.x - rhs, lhs.y - rhs);
    }
    
    public static Vector2 operator -(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.x - rhs, lhs.y - rhs);
    }
    
    public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.x + rhs.x, lhs.y + rhs.y);
    }
    
    public static Vector2 operator +(Vector2 lhs, int rhs)
    {
        return new Vector2(lhs.x + rhs, lhs.y + rhs);
    }
    
    public static Vector2 operator +(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.x + rhs, lhs.y + rhs);
    }

    public static Vector2 operator -(Vector2 vector) => new (-vector.x, -vector.y);

    public static implicit operator Vector3(Vector2 vector) => new Vector3(vector.x, vector.y, 0);
    public static implicit operator Vector4(Vector2 vector) => new Vector4(vector.x, vector.y, 0, 0);
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct Vector3
{
    [FieldOffset(0)]
    public float x;
    [FieldOffset(4)]
    public float y;
    [FieldOffset(8)]
    public float z; 
    
    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public static Vector3 Zero => new Vector3(0, 0, 0);

    public static Vector3 One => new Vector3(1, 1, 1);
    
    public static implicit operator System.Numerics.Vector3(Vector3 vector) =>
        new (vector.x, vector.y, vector.z);
    
    public static bool operator ==(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }
    
    public static bool operator !=(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != lhs.z;
    }
    
    public static Vector3 operator *(Vector3 lhs, int rhs)
    {
        return new Vector3(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
    }
    
    public static Vector3 operator *(Vector3 lhs, float rhs)
    {
        return new Vector3(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
    }
    
    public static Vector3 operator /(Vector3 lhs, int rhs)
    {
        return new Vector3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
    }
    
    public static Vector3 operator /(Vector3 lhs, float rhs)
    {
        return new Vector3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
    }

    public static Vector3 operator -(Vector3 lhs, int rhs)
    {
        return new Vector3(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
    }
    
    public static Vector3 operator -(Vector3 lhs, float rhs)
    {
        return new Vector3(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
    }
    
    public static Vector3 operator +(Vector3 lhs, int rhs)
    {
        return new Vector3(lhs.x - rhs, lhs.y + rhs, lhs.z + rhs);
    }

    public static Vector3 operator +(Vector3 lhs, float rhs)
    {
        return new Vector3(lhs.x - rhs, lhs.y + rhs, lhs.z + rhs);
    }
    
    public static Vector3 operator -(Vector3 vector) => new (-vector.x, -vector.y, -vector.z);
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct AlignedVector3
{
    [FieldOffset(0)]
    public float x;
    [FieldOffset(4)]
    public float y;
    [FieldOffset(8)]
    public float z;

    public AlignedVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public static implicit operator AlignedVector3(Vector3 vector) => new AlignedVector3(vector);
    
    public static implicit operator Vector3(AlignedVector3 vector) => new Vector3(vector.x, vector.y, vector.z);
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Vector4
{
    [FieldOffset(0)]
    public float x;
    [FieldOffset(4)]
    public float y;
    [FieldOffset(8)]
    public float z; 
    [FieldOffset(12)]
    public float w;

    [FieldOffset(0)]
    private Vector128<float> data;
    
    public Vector4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    
    public static Vector4 Zero => new Vector4(0, 0, 0, 0);
    public static Vector4 One => new Vector4(1, 1, 1, 1);

    public static implicit operator System.Numerics.Vector4(Vector4 vector) =>
        new (vector.x, vector.y, vector.z, vector.w);
    
    public static bool operator ==(Vector4 lhs, Vector4 rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
    }
    
    public static bool operator !=(Vector4 lhs, Vector4 rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != lhs.z || lhs.w != rhs.w;
    }
    
    
    
    public static Vector4 operator *(Vector4 lhs, int rhs)
    {
        return new Vector4(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
    }
    
    public static Vector4 operator *(Vector4 lhs, float rhs)
    {
        return new Vector4(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
    }
    
    public static Vector4 operator /(Vector4 lhs, int rhs)
    {
        return new Vector4(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs, lhs.w / rhs);
    }
    
    public static Vector4 operator /(Vector4 lhs, float rhs)
    {
        return new Vector4(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs, lhs.w / rhs);
    }

    public static Vector4 operator -(Vector4 lhs, int rhs)
    {
        return new Vector4(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs, lhs.w - rhs);
    }
    
    public static Vector4 operator -(Vector4 lhs, float rhs)
    {
        return new Vector4(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs, lhs.w - rhs);
    }
    
    public static Vector4 operator +(Vector4 lhs, int rhs)
    {
        return new Vector4(lhs.x - rhs, lhs.y + rhs, lhs.z + rhs, lhs.w + rhs);
    }

    public static Vector4 operator +(Vector4 lhs, float rhs)
    {
        return new Vector4(lhs.x - rhs, lhs.y + rhs, lhs.z + rhs, lhs.w + rhs);
    }
    
    public static Vector4 operator -(Vector4 vector) => new (-vector.x, -vector.y, -vector.z, -vector.w);
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Rect
{
    [FieldOffset(0)]
    public float x;
    [FieldOffset(4)]
    public float y;
    [FieldOffset(8)]
    public float w; 
    [FieldOffset(12)]
    public float h;

    [FieldOffset(0)]
    private Vector128<float> data;
    
    public float Width => w;
    public float Height => h;
    
    public Rect(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.w = width;
        this.h = height;
    }
    
    public static Rect Zero => new Rect(0, 0, 0, 0);
    public static Rect One => new Rect(0, 0, 1, 1);
    
    public static bool operator ==(Rect lhs, Rect rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.w == rhs.w && lhs.h == rhs.h;
    }
    
    public static bool operator !=(Rect lhs, Rect rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.w != lhs.w || lhs.h != rhs.h;
    }
    
    public Vector2 Max => new Vector2(Math.Max(this.x, this.x + this.w), Math.Max(this.y, this.y + this.h));
    public Vector2 Min => new Vector2(Math.Min(this.x, this.x + this.w), Math.Min(this.y, this.y + this.h));

    public void Contain(Vector2 point)
    {
        Vector2 min = this.Min;
        Vector2 max = this.Max;
        
        Vector2 new_min = new Vector2(
            Math.Min(min.x, point.x),
            Math.Min(min.y, point.y));
        
        Vector2 new_max = new Vector2(
            Math.Max(max.x, point.x),
            Math.Max(max.y, point.y));
        
        this = new Rect(
            new_min.x, 
            new_min.y, 
            new_max.x - new_min.x, 
            new_max.y - new_min.y);
    }

    public bool Contains(Vector2 point)
    {
        Vector2 min = this.Min;
        Vector2 max = this.Max;

        
        
        if (point.x < min.x || point.y < min.y ||
            point.x >= max.x || point.y >= max.y)
        {
            return false;
        }
        
        return true;
    }

    public bool Overlaps(Rect rect)
    {
        Vector2 min = this.Min;
        Vector2 max = this.Max;
        
        Vector2 rect_min = rect.Min;
        Vector2 rect_max = rect.Max;

        if (rect_max.x <= min.x || rect_min.x >= max.x ||
            rect_max.y <= min.y || rect_min.y >= max.y)
        {
            return false;
        }
        
        return true;
    }

    public Rect? GetOverlap(Rect rect)
    {
        Vector2 min = this.Min;
        Vector2 max = this.Max;
        
        Vector2 rect_min = rect.Min;
        Vector2 rect_max = rect.Max;

        if (rect_max.x <= min.x || rect_min.x >= max.x ||
            rect_max.y <= min.y || rect_min.y >= max.y)
        {
            return null;
        }

        Vector2 overlap_min = new Vector2(
            Math.Min(min.x, rect_min.x),
            Math.Min(min.y, rect_min.y));
        
        Vector2 overlap_max = new Vector2(
            Math.Max(max.x, rect_max.x),
            Math.Max(max.y, rect_max.y));
        
        return new Rect(
            overlap_min.x, 
            overlap_min.y, 
            overlap_max.x - overlap_min.x, 
            overlap_max.y - overlap_min.y);
    }
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct IVector2
{
    public int x, y;

    public IVector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static IVector2 Zero => new IVector2(0, 0);
    
    
    public static IVector2 One => new IVector2(1, 1);
    
    public static bool operator ==(IVector2 lhs, IVector2 rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    
    public static bool operator !=(IVector2 lhs, IVector2 rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y;
    }

    public static IVector2 operator *(IVector2 lhs, int rhs)
    {
        return new IVector2(lhs.x * rhs, lhs.y * rhs);
    }
    
    public static IVector2 operator /(IVector2 lhs, int rhs)
    {
        return new IVector2(lhs.x / rhs, lhs.y / rhs);
    }
    
    public static IVector2 operator -(IVector2 lhs, IVector2 rhs)
    {
        return new IVector2(lhs.x - rhs.x, lhs.y - rhs.y);
    }

    public static IVector2 operator -(IVector2 lhs, int rhs)
    {
        return new IVector2(lhs.x - rhs, lhs.y - rhs);
    }
    
    public static IVector2 operator +(IVector2 lhs, IVector2 rhs)
    {
        return new IVector2(lhs.x + rhs.x, lhs.y + rhs.y);
    }
    
    public static IVector2 operator +(IVector2 lhs, int rhs)
    {
        return new IVector2(lhs.x + rhs, lhs.y + rhs);
    }

    public static IVector2 operator -(IVector2 vector) => new (-vector.x, -vector.y);
    
    public static implicit operator IVector2(Vector2 vector) => new IVector2((int)vector.x, (int)vector.y);
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct IRect
{
    [FieldOffset(0)]
    public int x;
    [FieldOffset(4)]
    public int y;
    [FieldOffset(8)]
    public int w; 
    [FieldOffset(12)]
    public int h;

    [FieldOffset(0)]
    private Vector128<int> data;
    
    public int Width => w;
    public int Height => h;
    
    public IRect(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.w = width;
        this.h = height;
    }
    
    public static IRect Zero => new IRect(0, 0, 0, 0);
    public static IRect One => new IRect(0, 0, 1, 1);
    
    public static bool operator ==(IRect lhs, IRect rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.w == rhs.w && lhs.h == rhs.h;
    }
    
    public static bool operator !=(IRect lhs, IRect rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.w != lhs.w || lhs.h != rhs.h;
    }
    
    public IVector2 Max => new IVector2(Math.Max(this.x, this.x + this.w), Math.Max(this.y, this.y + this.h));
    public IVector2 Min => new IVector2(Math.Min(this.x, this.x + this.w), Math.Min(this.y, this.y + this.h));

    public void Contain(IVector2 point)
    {
        IVector2 min = this.Min;
        IVector2 max = this.Max;
        
        IVector2 new_min = new IVector2(
            Math.Min(min.x, point.x),
            Math.Min(min.y, point.y));
        
        IVector2 new_max = new IVector2(
            Math.Max(max.x, point.x),
            Math.Max(max.y, point.y));
        
        this = new IRect(
            new_min.x, 
            new_min.y, 
            new_max.x - new_min.x, 
            new_max.y - new_min.y);
    }

    public bool Contains(IVector2 point)
    {
        IVector2 min = this.Min;
        IVector2 max = this.Max;
        
        if (point.x < min.x || point.y < min.y ||
            point.x >= max.x || point.y >= max.y)
        {
            return false;
        }
        
        return true;
    }

    public bool Overlaps(IRect rect)
    {
        IVector2 min = this.Min;
        IVector2 max = this.Max;
        
        IVector2 rect_min = rect.Min;
        IVector2 rect_max = rect.Max;

        if (rect_max.x <= min.x || rect_min.x >= max.x ||
            rect_max.y <= min.y || rect_min.y >= max.y)
        {
            return false;
        }
        
        return true;
    }

    public IRect? GetOverlap(IRect rect)
    {
        IVector2 min = this.Min;
        IVector2 max = this.Max;
        
        IVector2 rect_min = rect.Min;
        IVector2 rect_max = rect.Max;

        if (rect_max.x <= min.x || rect_min.x >= max.x ||
            rect_max.y <= min.y || rect_min.y >= max.y)
        {
            return null;
        }

        IVector2 overlap_min = new IVector2(
            Math.Min(min.x, rect_min.x),
            Math.Min(min.y, rect_min.y));
        
        IVector2 overlap_max = new IVector2(
            Math.Max(max.x, rect_max.x),
            Math.Max(max.y, rect_max.y));
        
        return new IRect(
            overlap_min.x, 
            overlap_min.y, 
            overlap_max.x - overlap_min.x, 
            overlap_max.y - overlap_min.y);
    }
}