using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace WaffleEngine;

public static class WMath
{
    public static int Mod(int value, int mod) => (value % mod + mod) % mod;
}

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
    public static implicit operator Vector2(System.Numerics.Vector2 vector) =>
        new (vector.X, vector.Y);
    
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

[StructLayout(LayoutKind.Sequential, Size = 8)]
public record struct Vector2Int
{
    public int x, y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2Int Zero => new Vector2Int(0, 0);
    
    public static Vector2Int One => new Vector2Int(1, 1);

    public static Vector2Int operator *(Vector2Int lhs, int rhs)
    {
        return new Vector2Int(lhs.x * rhs, lhs.y * rhs);
    }
    
    public static Vector2Int operator /(Vector2Int lhs, int rhs)
    {
        return new Vector2Int(lhs.x / rhs, lhs.y / rhs);
    }
    
    public static Vector2Int operator -(Vector2Int lhs, Vector2Int rhs)
    {
        return new Vector2Int(lhs.x - rhs.x, lhs.y - rhs.y);
    }

    public static Vector2Int operator -(Vector2Int lhs, int rhs)
    {
        return new Vector2Int(lhs.x - rhs, lhs.y - rhs);
    }
    
    public static Vector2Int operator +(Vector2Int lhs, Vector2Int rhs)
    {
        return new Vector2Int(lhs.x + rhs.x, lhs.y + rhs.y);
    }
    
    public static Vector2Int operator +(Vector2Int lhs, int rhs)
    {
        return new Vector2Int(lhs.x + rhs, lhs.y + rhs);
    }

    public static Vector2 operator -(Vector2Int vector) => new (-vector.x, -vector.y);
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

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct Vector3Int
{
    [FieldOffset(0)]
    public int x;
    [FieldOffset(4)]
    public int y;
    [FieldOffset(8)]
    public int z; 
    
    public Vector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public static Vector3Int Zero => new Vector3Int(0, 0, 0);

    public static Vector3Int One => new Vector3Int(1, 1, 1);
    
    public static bool operator ==(Vector3Int lhs, Vector3Int rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }
    
    public static bool operator !=(Vector3Int lhs, Vector3Int rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != lhs.z;
    }
    
    public static Vector3Int operator *(Vector3Int lhs, int rhs)
    {
        return new Vector3Int(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
    }
    
    public static Vector3Int operator /(Vector3Int lhs, int rhs)
    {
        return new Vector3Int(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
    }

    public static Vector3Int operator -(Vector3Int lhs, int rhs)
    {
        return new Vector3Int(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
    }
    
    public static Vector3Int operator +(Vector3Int lhs, int rhs)
    {
        return new Vector3Int(lhs.x - rhs, lhs.y + rhs, lhs.z + rhs);
    }
    
    public static Vector3Int operator -(Vector3Int vector) => new (-vector.x, -vector.y, -vector.z);
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