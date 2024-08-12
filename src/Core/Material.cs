using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public abstract class Material
{
    public Shader Shader;
    public Texture Texture;

    public abstract void Enable(Camera? camera);
    public abstract void Disable();
}

public class DefaultSpriteMaterial : Material
{
    private int _view_uniform_mat;
    private int _projection_uniform_mat;
    private int _texture_sampler_uniform;

    public DefaultSpriteMaterial(Shader shader, Texture texture)
    {
        Shader = shader;
        Texture = texture;

        _view_uniform_mat = Shader.GetUniformLocation("view");
        _projection_uniform_mat = Shader.GetUniformLocation("projection");
        _texture_sampler_uniform = Shader.GetUniformLocation("_texture");
    }

    public override void Enable(Camera? camera)
    {
        Shader.Enable();

        if (camera != null)
        {
            GL.UniformMatrix4f(_view_uniform_mat, 1, false, camera.TranslationMatrix);
            GL.UniformMatrix4f(_projection_uniform_mat, 1, false, camera.ProjectionMatrix);
        }

        GL.Uniform1i(_texture_sampler_uniform, 0);

        Texture.BindTo(TextureUnit.Texture0);
    }

    public override void Disable()
    {
        Shader.Disable();
        Texture.Unbind();
    }
}

public class DefaultFullscreenMaterial : Material
{
    private int _texture_sampler_uniform;

    public DefaultFullscreenMaterial(Shader shader, RenderTexture texture)
    {
        Shader = shader;
        Texture = texture;

        _texture_sampler_uniform = Shader.GetUniformLocation("_texture");
    }

    public override void Enable(Camera? camera)
    {
        Shader.Enable();

        GL.Uniform1i(_texture_sampler_uniform, 0);

        Texture.BindTo(TextureUnit.Texture0);
    }
    public override void Disable()
    {
        Shader.Disable();
        Texture.Unbind();
    }
}