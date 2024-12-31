using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public class QuadMesh : Mesh
{
    public Material Material;
    
    private int _element_buffer_object;

    public static QuadMesh Create(Material material)
    {
        QuadMesh mesh = new QuadMesh();
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

        return mesh;
    }

    public void SetTransform(Transform transform)
    {
        float[] quad_vertices = {
            //Position          Texture coordinates
            transform.Position.X + transform.Scale.X * 0.5f,  transform.Position.Y + transform.Scale.Y * 0.5f, 0.0f, 1.0f, 1.0f, // top right
            transform.Position.X + transform.Scale.X * 0.5f,  transform.Position.Y - transform.Scale.Y * 0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            transform.Position.X - transform.Scale.X * 0.5f,  transform.Position.Y - transform.Scale.Y * 0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            transform.Position.X - transform.Scale.X * 0.5f,  transform.Position.Y + transform.Scale.Y * 0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertex_buffer_object);
        GL.BufferData(BufferTarget.ArrayBuffer, quad_vertices, BufferUsage.DynamicDraw);
    }

    public override void Unload()
    {
        GL.DeleteBuffer(_element_buffer_object);
        
        base.Unload();
    }
}