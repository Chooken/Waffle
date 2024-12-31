using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.PixelPerfect;

public class PixelPerfectRenderer
{
    private static FullscreenMesh _mesh;
    private static RenderTexture _texture;
    private static Camera _camera;
    private static float _cached_fov;
    private static int _cached_pixels_per_unit;

    public static void Set(Camera camera)
    {
        _camera = camera;
        _cached_fov = _camera.Fov;
        _cached_pixels_per_unit = camera.PixelsPerUnit;

        if (_mesh != null)
        {
            _texture.UpdateRenderTexture(
                System.Math.Min(
                    (int)MathF.Ceiling(camera.Width * camera.PixelsPerUnit * 0.5f) * 2 + 2, Window.Width),
                System.Math.Min(
                    (int)MathF.Ceiling(camera.Height * 16 * 0.5f) * 2 + 2, Window.Height));
            return;
        }

        _texture = new RenderTexture(
                System.Math.Min(
                    (int)MathF.Ceiling(camera.Width * camera.PixelsPerUnit * 0.5f) * 2 + 2, Window.Width),
                System.Math.Min(
                    (int)MathF.Ceiling(camera.Height * 16 * 0.5f) * 2 + 2, Window.Height));

        Shader shader = Shader.Get("core", "pixel_perfect");

        PixelPerfectMaterial material = new PixelPerfectMaterial(shader, _texture);

        _mesh = FullscreenMesh.Create(material);
    }

    public static void Start(Camera camera)
    {
        if (_camera != camera || 
            _cached_fov != camera.Fov || 
            _cached_pixels_per_unit != camera.PixelsPerUnit)
            Set(camera);

        RenderTarget.Set(_texture);

        RenderTarget.Clear();

        _camera.SnapToGrid = true;

        _camera.SetFov((float)_texture.Width / _camera.PixelsPerUnit, (float)_texture.Height / _camera.PixelsPerUnit);
    }

    public static void Stop()
    {
        RenderTarget.Screen();

        _camera.SetFov(_cached_fov);
        _camera.SnapToGrid = false;

        _mesh.Bind();
        _mesh.Material.Enable(_camera);

        unsafe
        {
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
        }

        _mesh.Material.Disable();
    }
}