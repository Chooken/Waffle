using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public class SpriteMesh : Mesh
{
    public Material Material;
    public int VertexDataVboId => _vertex_data_vbo_id;
    
    private int _element_buffer_object;
    private int _vertex_data_vbo_id;
    private int _vertex_data_vbo_size;

    public static SpriteMesh Create(Material material)
    {
        SpriteMesh mesh = new SpriteMesh();
        mesh.Material = material;
        
        float[] quad_vertices = {
            //Position          Texture coordinates
            0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };
        uint[] quad_indices = new uint[6];

        quad_indices[0] = 0;
        quad_indices[1] = 1;
        quad_indices[2] = 2;
        
        quad_indices[3] = 0;
        quad_indices[4] = 2;
        quad_indices[5] = 3;

        uint pos_location = (uint)mesh.Material.Shader.GetAttribLocation("vertex_pos");
        uint uv_location = (uint)mesh.Material.Shader.GetAttribLocation("vertex_uv");
        
        uint transform_attrib_location = (uint)mesh.Material.Shader.GetAttribLocation("transform");
        uint uv_offset_attrib_location = (uint)mesh.Material.Shader.GetAttribLocation("uv_offset");
        uint uv_size_attrib_location = (uint)mesh.Material.Shader.GetAttribLocation("uv_size");

        mesh._vertex_buffer_object = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._vertex_buffer_object);
        GL.BufferData(BufferTarget.ArrayBuffer, quad_vertices, BufferUsage.StaticDraw);

        mesh._vertex_array_object = GL.GenVertexArray();
        GL.BindVertexArray(mesh._vertex_array_object);
        
        mesh._element_buffer_object = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._element_buffer_object);
        GL.BufferData(BufferTarget.ElementArrayBuffer, quad_indices, BufferUsage.StaticDraw);
        
        GL.EnableVertexAttribArray(pos_location);
        GL.VertexAttribPointer(pos_location, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        
        GL.EnableVertexAttribArray(uv_location);
        GL.VertexAttribPointer(uv_location, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        
        // Position VBO
        mesh._vertex_data_vbo_id = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._vertex_data_vbo_id);

        unsafe
        {
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 20 * 1024, null,
                BufferUsage.StreamDraw);
        }

        GL.EnableVertexAttribArray(transform_attrib_location);
        GL.VertexAttribPointer(transform_attrib_location, 4, VertexAttribPointerType.Float, false, 20 * sizeof(float), 0);
        GL.VertexAttribDivisor(transform_attrib_location, 1);
        
        GL.EnableVertexAttribArray(transform_attrib_location + 1);
        GL.VertexAttribPointer(transform_attrib_location + 1, 4, VertexAttribPointerType.Float, false, 20 * sizeof(float), 4 * sizeof(float));
        GL.VertexAttribDivisor(transform_attrib_location + 1, 1);
        
        GL.EnableVertexAttribArray(transform_attrib_location + 2);
        GL.VertexAttribPointer(transform_attrib_location + 2, 4, VertexAttribPointerType.Float, false, 20 * sizeof(float), 8 * sizeof(float));
        GL.VertexAttribDivisor(transform_attrib_location + 2, 1);
        
        GL.EnableVertexAttribArray(transform_attrib_location + 3);
        GL.VertexAttribPointer(transform_attrib_location + 3, 4, VertexAttribPointerType.Float, false, 20 * sizeof(float), 12 * sizeof(float));
        GL.VertexAttribDivisor(transform_attrib_location + 3, 1);
        
        GL.EnableVertexAttribArray(uv_offset_attrib_location);
        GL.VertexAttribPointer(uv_offset_attrib_location, 2, VertexAttribPointerType.Float, false, 20 * sizeof(float), 16 * sizeof(float));
        GL.VertexAttribDivisor(uv_offset_attrib_location, 1);
        
        GL.EnableVertexAttribArray(uv_size_attrib_location);
        GL.VertexAttribPointer(uv_size_attrib_location, 2, VertexAttribPointerType.Float, false, 20 * sizeof(float), 18 * sizeof(float));
        GL.VertexAttribDivisor(uv_size_attrib_location, 1);

        return mesh;
    }

    public void UpdateVertexData((Matrix4x4 transform, Vector2 offset, Vector2 size)[] buffer)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexDataVboId);

        if (buffer.Length > _vertex_data_vbo_size)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, buffer, BufferUsage.StreamDraw);
            _vertex_data_vbo_size = buffer.Length;
        }
        else
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, buffer);
    }

    public override void Unload()
    {
        GL.DeleteBuffer(_element_buffer_object);
        GL.DeleteBuffer(_vertex_data_vbo_id);
        
        base.Unload();
    }
}