using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public abstract class Material
{
    public Shader Shader;
    public Texture Texture;

    public abstract void Enable(Camera camera);
    public abstract void Disable();

    public void SetUniformMatrix(int location, Matrix4x4 matrix)
    {
        GL.UniformMatrix4f(location, 1, false, matrix);
    }

    public void SetUniformFloat(int location, float value)
    {
        GL.Uniform1f(location, value);
    }

    public void SetUniformInt(int location, int value)
    {
        GL.Uniform1i(location, value);
    }

    public void SetUniformVec(int location, Vector2 value)
    {
        GL.Uniform2f(location, 1, value);
    }

    public void SetUniformVec(int location, Vector3 value)
    {
        GL.Uniform3f(location, 1, value);
    }

    public void SetUniformVec(int location, Vector4 value)
    {
        GL.Uniform4f(location, 1, value);
    }
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

    public override void Enable(Camera camera)
    {
        Shader.Enable();


        if (camera != null)
        {
            SetUniformMatrix(_view_uniform_mat, camera.TranslationMatrix);
            SetUniformMatrix(_projection_uniform_mat, camera.ProjectionMatrix);
        }

        SetUniformInt(_texture_sampler_uniform, 0);

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

    public override void Enable(Camera camera)
    {
        Shader.Enable();

        SetUniformInt(_texture_sampler_uniform, 0);

        Texture.BindTo(TextureUnit.Texture0);
    }

    public override void Disable()
    {
        Shader.Disable();
        Texture.Unbind();
    }
}

public class DefaultTilemapMaterial : Material
{
    private int _view_uniform_mat;
    private int _projection_uniform_mat;
    private int _texture_sampler_uniform;
    private int _tile_ids_uniform;
    private int _chunk_width_uniform;
    private int _pixels_per_unit_uniform;
    private int _lod_uniform;

    private int _tile_id_vbo_id;
    private int _tile_id_texture_id;

    private int _size;

    private float _texture_pixels_per_unit = 16;


    public DefaultTilemapMaterial(Shader shader, Texture texture, int size)
    {
        Shader = shader;
        Texture = texture;

        _size = size;

        _view_uniform_mat = Shader.GetUniformLocation("view");
        _projection_uniform_mat = Shader.GetUniformLocation("projection");
        _texture_sampler_uniform = Shader.GetUniformLocation("_texture");
        _tile_ids_uniform = Shader.GetUniformLocation("tile_ids");

        _pixels_per_unit_uniform = Shader.GetUniformLocation("pixels_per_unit");

        // Tile ID Buffer
        _tile_id_vbo_id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, _tile_id_vbo_id);

        unsafe
        {
            GL.BufferData(BufferTarget.TextureBuffer, size * sizeof(int), null,
                BufferUsage.StaticDraw);
        }

        _tile_id_texture_id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureBuffer, _tile_id_texture_id);
        GL.TexBuffer(TextureTarget.TextureBuffer, SizedInternalFormat.R32i, _tile_id_vbo_id);
    }

    public override void Enable(Camera camera)
    {
        Shader.Enable();


        if (camera != null)
        {
            SetUniformMatrix(_view_uniform_mat, camera.TranslationMatrix);
            SetUniformMatrix(_projection_uniform_mat, camera.ProjectionMatrix);
        }

        Texture.BindTo(TextureUnit.Texture0);

        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.TextureBuffer, _tile_id_texture_id);

        SetUniformInt(_texture_sampler_uniform, 0);
        SetUniformInt(_tile_ids_uniform, 1);
        SetUniformFloat(_pixels_per_unit_uniform, _texture_pixels_per_unit);
    }

    public void SetPixelsPerUnit(float ppu)
    {
        _texture_pixels_per_unit = ppu;
    }

    public void SetTiles(int[] tile_ids)
    {
        GL.BindBuffer(BufferTarget.TextureBuffer, _tile_id_vbo_id);
        GL.BufferSubData(BufferTarget.TextureBuffer, 0, tile_ids);
    }

    public void SetTiles(int[] tile_ids, nint offset)
    {
        if (offset + tile_ids.Length > _size || offset < 0)
        {
            Log.Error("Trying to set tiles past the size of the tilemap.");
            return;
        }

        GL.BindBuffer(BufferTarget.TextureBuffer, _tile_id_vbo_id);
        GL.BufferSubData(BufferTarget.TextureBuffer, offset * sizeof(int), tile_ids);
    }

    public void SetTile(int tile_id, nint position)
    {
        if (position > _size || position < 0)
        {
            Log.Error("Trying to set tiles past the size of the tilemap.");
            return;
        }

        GL.BindBuffer(BufferTarget.TextureBuffer, _tile_id_vbo_id);
        GL.BufferSubData(BufferTarget.TextureBuffer, position * sizeof(int), 1 * sizeof(int), tile_id);
    }

    public override void Disable()
    {
        Shader.Disable();

        Texture.Unbind();

        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.TextureBuffer, 0);
    }

    public void Dispose()
    {

        GL.DeleteBuffer(_tile_id_vbo_id);
        GL.DeleteTexture(_tile_id_texture_id);
    }
}