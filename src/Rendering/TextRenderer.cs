using System.Numerics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;

namespace WaffleEngine;

public class TextRenderer
{
    private string _text;
    private readonly Font _font;

    private readonly QuadMesh _quad_mesh;
    
    private readonly Shader _shader;
    private readonly Material _material;

    private readonly int _view_uniform_mat;
    private readonly int _projection_uniform_mat;

    private readonly int _pos_vbo_id;
    private readonly int _scale_vbo_id;
    private readonly int _data_offset_vbo_id;

    private readonly int _bezier_sampler_location;
    private readonly int _bezier_tbo;
    private readonly int _bezier_tex;
    
    private readonly int _metadata_sampler_location;
    private readonly int _metadata_tbo;
    private readonly int _metadata_tex;
    
    private Vector2[] _position_array;
    private Vector2[] _scale_array;
    private int[] _data_offset_array;
     
    public TextRenderer(string text, Font font)
    {
        _text = text;
        _font = font;
        _shader = new Shader(
            $"{Environment.CurrentDirectory}/assets/shaders/text_shader.vert", 
            $"{Environment.CurrentDirectory}/assets/shaders/text_shader.frag");

        _material = new DefaultSpriteMaterial(_shader, null);
        
        uint pos_attribute_location = (uint)_shader.GetAttribLocation("Pos");
        uint size_attribute_location = (uint)_shader.GetAttribLocation("Size");
        uint data_offset_attribute_location = (uint)_shader.GetAttribLocation("DataOffset");

        _bezier_sampler_location = _shader.GetUniformLocation("texture1");
        _metadata_sampler_location = _shader.GetUniformLocation("texture2");

        _view_uniform_mat = _shader.GetUniformLocation("view_mat");
        _projection_uniform_mat = _shader.GetUniformLocation("projection_mat");

        uint vert_pos_attrib_location = (uint)_shader.GetAttribLocation("vertex_pos");
        uint vert_uv_attrib_location = (uint)_shader.GetAttribLocation("vertex_uv");

        _quad_mesh = QuadMesh.Create(_material);
        
        // Bezier Buffer Texture
        _bezier_tbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, _bezier_tbo);
        
        GL.BufferData(BufferTarget.TextureBuffer, _font.BezierPoints, BufferUsage.StaticDraw);

        _bezier_tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureBuffer, _bezier_tex);
        
        GL.TexBuffer(TextureTarget.TextureBuffer, SizedInternalFormat.Rg32f, _bezier_tbo);
        
        
        // Metadata Buffer Texture
        _metadata_tbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, _metadata_tbo);
        
        GL.BufferData(BufferTarget.TextureBuffer, _font.GlyphMetaData, BufferUsage.StaticDraw);

        _metadata_tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureBuffer, _metadata_tex);
        
        GL.TexBuffer(TextureTarget.TextureBuffer, SizedInternalFormat.R32i, _metadata_tbo);
        
        // Position VBO
        _pos_vbo_id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _pos_vbo_id);
        
        GL.EnableVertexAttribArray(pos_attribute_location);
        GL.VertexAttribPointer(pos_attribute_location, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.VertexAttribDivisor(pos_attribute_location, 1);
        
        
        // Size VBO
        _scale_vbo_id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _scale_vbo_id);
        
        GL.EnableVertexAttribArray(size_attribute_location);
        GL.VertexAttribPointer(size_attribute_location, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.VertexAttribDivisor(size_attribute_location, 1);
        
        
        // Data Offset VBO
        _data_offset_vbo_id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _data_offset_vbo_id);
        
        GL.EnableVertexAttribArray(data_offset_attribute_location);
        GL.VertexAttribPointer(data_offset_attribute_location, 1, VertexAttribPointerType.Float, false, sizeof(int), 0);
        GL.VertexAttribDivisor(data_offset_attribute_location, 1);
        
        GenerateGlyphsList();
    }

    private void GenerateGlyphsList()
    {
        int count = _text.Count(character => !char.IsWhiteSpace(character));

        _position_array = new Vector2[count];
        _scale_array = new Vector2[count];
        _data_offset_array = new int[count];

        
        float x_width = 13f;
        float x = -x_width * 0.5f;

        float y = 3;

        int glyphs_generated = 0;

        foreach (var character in _text)
        {
            if (character == ' ')
            {
                x += 0.333f;
                continue;
            }

            if (character == '\n')
            {
                x = -x_width * 0.5f;
                y -= 1.3f;
            }
            
            GlyphData data = _font.GetGlyphData(character);

            _position_array[glyphs_generated] = new Vector2(x + data.OffsetX, y + data.OffsetY);
            _scale_array[glyphs_generated] = data.Size;
            _data_offset_array[glyphs_generated] = data.ContourDataOffset;

            x += data.AdvanceWidth;

            if (x > x_width * 0.5f)
            {
                x = -x_width * 0.5f;
                y -= 1.2f;
            }

            glyphs_generated++;
        }
        
        // Set up instance data
        _quad_mesh.Bind();

        
        // Position VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, _pos_vbo_id);
        GL.BufferData(BufferTarget.ArrayBuffer, _position_array, BufferUsage.StreamDraw);
        
        
        // Size VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, _scale_vbo_id);
        GL.BufferData(BufferTarget.ArrayBuffer, _scale_array, BufferUsage.StreamDraw);
        
        
        // Data Offset VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, _data_offset_vbo_id);
        GL.BufferData(BufferTarget.ArrayBuffer, _data_offset_array, BufferUsage.StreamDraw);
        
        // Disable Vertex Array
        GL.BindVertexArray(0);
    }

    public void UpdateText(string text)
    {
        if (_text == text)
            return;
        
        _text = text;
        
        GenerateGlyphsList();
    }

    public unsafe void Render(Camera camera, float render_percentage = 100)
    {
        // Start to instanced render
        
        _shader.Enable();
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        Matrix4x4 mat_view = camera.TranslationMatrix;
        Matrix4x4 mat_projection = camera.ProjectionMatrix;
        
        _quad_mesh.Bind();
        
        GL.UniformMatrix4f(_view_uniform_mat, 1, false, mat_view);
        GL.UniformMatrix4f(_projection_uniform_mat, 1, false, mat_projection);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureBuffer, _bezier_tex);
        
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.TextureBuffer, _metadata_tex);
        
        GL.Uniform1i(_bezier_sampler_location, 0);
        GL.Uniform1i(_metadata_sampler_location, 1);
        
        GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null, (int)(_position_array.Length * float.Clamp(render_percentage, 0, 1)));
        
        GL.BindTexture(TextureTarget.TextureBuffer, 0);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureBuffer, 0);
        
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        
        _shader.Disable();
    }

    public void Unload()
    {
        GL.DeleteBuffer(_pos_vbo_id);
        GL.DeleteBuffer(_scale_vbo_id);
        GL.DeleteBuffer(_data_offset_vbo_id);
        GL.DeleteBuffer(_bezier_tbo);
        GL.DeleteBuffer(_metadata_tbo);
        
        _quad_mesh.Unload();
    }
}