using System.Numerics;
using OpenTK.Mathematics;

namespace WaffleEngine
{
    public class Camera
    {
        private Matrix4x4 projection_mat;
        private Matrix4x4 translation_mat;

        public float Fov { get; private set; }

        public Matrix4x4 ProjectionMatrix => projection_mat;
        public Matrix4x4 TranslationMatrix => translation_mat;

        public Camera(float x, float y, float rotation)
        {
            Fov = 8f;

            projection_mat = Matrix4x4.CreateOrthographic((float)Window.RenderWidth / Window.RenderHeight * Fov, Fov, 0.1f, 100.0f);
            translation_mat = Matrix4x4.CreateTranslation(-x, -y, -10);

            Window.WindowResizeEvent += OnWindowResize;
        }

        ~Camera()
        {
            Window.WindowResizeEvent -= OnWindowResize;
        }

        private void OnWindowResize()
        {
            projection_mat = Matrix4x4.CreateOrthographic((float)Window.RenderWidth / Window.RenderHeight * Fov, Fov, 0.1f, 100.0f);
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

        public void Zoom(float fov_delta)
        {
            Fov -= fov_delta;
            
            projection_mat = Matrix4x4.CreateOrthographic((float)Window.RenderWidth / Window.RenderHeight * Fov, Fov, 0.1f, 100.0f);
        }
    }
}
