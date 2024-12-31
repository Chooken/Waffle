using System.Numerics;
using System.Runtime.Intrinsics;
using OpenTK.Mathematics;

namespace WaffleEngine
{
    public class Camera
    {
        private Matrix4x4 translation_mat;

        public float Fov => Height;

        public float Width { 
            get {
                return (float)Window.RenderWidth / Window.RenderHeight * Height;
            } 
        }
        public float Height { get; private set; }

        public Matrix4x4 ProjectionMatrix => Matrix4x4.CreateOrthographic(Width, Height, 0.1f, 100.0f);
        public Matrix4x4 TranslationMatrix {

            get {

                if (!SnapToGrid)
                    return translation_mat;

                Matrix4x4 matrix = translation_mat;

                matrix.M41 = MathF.Floor(matrix.M41 * PixelsPerUnit) / PixelsPerUnit;
                matrix.M42 = MathF.Floor(matrix.M42 * PixelsPerUnit) / PixelsPerUnit;

                return matrix;
            }

            private set { translation_mat = value; }
        }
        public Matrix4x4 RawTranslationMatrix => translation_mat;

        public bool SnapToGrid = false;

        public int PixelsPerUnit = 16;

        public Camera(float x, float y, float rotation)
        {
            Height = 8f;

            translation_mat = Matrix4x4.CreateTranslation(-x, -y, -10);
        }

        public void SetPosition(float x, float y)
        {
            translation_mat.M41 = -x;
            translation_mat.M42 = -y;
        }

        public void Move(float x, float y)
        {
            translation_mat.M41 -= x;
            translation_mat.M42 -= y;
        }

        public void Move(System.Numerics.Vector2 vector)
        {
            translation_mat.M41 -= vector.X;
            translation_mat.M42 -= vector.Y;
        }

        public void SetFov(float fov)
        {
            Height = fov;
        }

        public void SetFov(float width, float height)
        {
            Height = height;
        }

        public void Zoom(float fov_delta)
        {
            Height -= fov_delta;
        }
    }
}
