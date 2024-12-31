using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public class TilemapMesh : Mesh
{
    public Material Material;

    private int _element_buffer_object;

    public static TilemapMesh Create(Material material, Vector2 position, int width, int height)
    {
        TilemapMesh mesh = new TilemapMesh();
        mesh.Material = material;

        float[] quad_vertices = {
            //Position          Texture coordinates
             (float)width / 2 + position.X,  (float)height / 2 + position.Y, 0.0f, width - float.Epsilon, height - float.Epsilon, // top right
             (float)width / 2 + position.X, -(float)height / 2 + position.Y, 0.0f, width - float.Epsilon, 0.0f, // bottom right
            -(float)width / 2 + position.X, -(float)height / 2 + position.Y, 0.0f, 0.0f,  0.0f, // bottom left
            -(float)width / 2 + position.X,  (float)height / 2 + position.Y, 0.0f, 0.0f,  height - float.Epsilon  // top left
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

        uint tile_uvs_uniform_location = (uint)mesh.Material.Shader.GetUniformLocation("tile_uvs");
        uint texture_uniform_location = (uint)mesh.Material.Shader.GetUniformLocation("texture");

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

    public override void Unload()
    {
        GL.DeleteBuffer(_element_buffer_object);

        base.Unload();
    }
}