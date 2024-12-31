using OpenTK.Graphics.OpenGL;
using System.Numerics;

namespace WaffleEngine.PixelPerfect;

public class PixelPerfectMaterial : Material
{
    private int _texture_sampler_uniform;
    private int _ratio_uniform;
    private int _offset_uniform;

    public PixelPerfectMaterial(Shader shader, RenderTexture texture)
    {
        Shader = shader;
        Texture = texture;

        _texture_sampler_uniform = Shader.GetUniformLocation("_texture");
        _ratio_uniform = Shader.GetUniformLocation("_ratio");
        _offset_uniform = Shader.GetUniformLocation("_offset");
    }

    public override void Enable(Camera camera)
    {
        if (camera == null)
            return;

        Shader.Enable();

        float smooth_fov_x = ((float)System.Math.Min((int)MathF.Ceiling(camera.Width * camera.PixelsPerUnit * 0.5f) * 2, Window.Width) / 16 - camera.Width) / camera.Width;
        float smooth_fov_y = ((float)System.Math.Min((int)MathF.Ceiling(camera.Height * camera.PixelsPerUnit * 0.5f) * 2, Window.Height) / 16 - camera.Height) / camera.Height;

        Vector2 calculated_ratio = new Vector2(
            1f + (2f / camera.Width / camera.PixelsPerUnit) + smooth_fov_x,
            1f + (2f / camera.Height / camera.PixelsPerUnit) + smooth_fov_y
        );

        Matrix4x4 matrix = Matrix4x4.Identity;
        matrix.M41 = Math.Mod(camera.TranslationMatrix.M41, 1f / camera.PixelsPerUnit) - (1f / camera.PixelsPerUnit);
        matrix.M42 = Math.Mod(camera.TranslationMatrix.M42, 1f / camera.PixelsPerUnit) - (1f / camera.PixelsPerUnit);
        Matrix4x4 project_space = Matrix4x4.Multiply(matrix, camera.ProjectionMatrix);

        Vector2 offset = new Vector2(
            project_space.M41,
            project_space.M42
        );

        SetUniformInt(_texture_sampler_uniform, 0);
        SetUniformVec(_ratio_uniform, calculated_ratio);
        SetUniformVec(_offset_uniform, offset);

        Texture.BindTo(TextureUnit.Texture0);
    }

    public override void Disable()
    {
        Shader.Disable();
        Texture.Unbind();
    }
}