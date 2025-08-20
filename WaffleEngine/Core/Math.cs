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
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Vector3
{
    [FieldOffset(0)]
    public float x;
    [FieldOffset(4)]
    public float y;
    [FieldOffset(8)]
    public float z; 

    //[FieldOffset(0)]
    //private Vector128<float> data;
    
    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public static Vector3 Zero => new Vector3(0, 0, 0);
    
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
}