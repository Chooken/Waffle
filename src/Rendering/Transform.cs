using System.Numerics;
using OpenTK.Mathematics;
using Vector3 = System.Numerics.Vector3;

namespace WaffleEngine;

public struct Transform
{
    public Vector3 Position => new Vector3(Matrix.M14, Matrix.M24, Matrix.M34);

    public Matrix4x4 Matrix = Matrix4x4.Identity;

    public Transform()
    {
    }

    public Transform Translate(float x, float y, float z)
    {
        Matrix.M14 += x;
        Matrix.M24 += y;
        Matrix.M34 += z;

        return this;
    }

    public Transform Translate2D(float x, float y)
    {
        Matrix.M14 += x;
        Matrix.M24 += y;
        Matrix.M34 = -Matrix.M24;

        return this;
    }

    public Transform SetPosition(float x, float y, float z)
    {
        Matrix.M14 = x;
        Matrix.M24 = y;
        Matrix.M34 = z;

        return this;
    }
    
    public Transform SetPosition2D(float x, float y)
    {
        Matrix.M14 = x;
        Matrix.M24 = y;
        Matrix.M34 = -Matrix.M24;

        return this;
    }

    public Transform Scale(float x, float y, float z)
    {
        Matrix4x4 scale_matrix4 = Matrix4x4.Identity;
        scale_matrix4.M11 = x;
        scale_matrix4.M22 = y;
        scale_matrix4.M33 = z;

        Matrix = Matrix4x4.Multiply(Matrix, scale_matrix4);

        return this;
    }

    public Transform SetScale(float x, float y, float z)
    {
        Matrix.M11 = x;
        Matrix.M22 = y;
        Matrix.M33 = z;

        return this;
    }
}