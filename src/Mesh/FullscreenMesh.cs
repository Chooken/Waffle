using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine;

public class FullscreenMesh : Mesh
{
    public Material Material;

    private int _element_buffer_object;

    public static FullscreenMesh Create(Material material)
    {
        FullscreenMesh mesh = new FullscreenMesh();
        mesh.Material = material;

        float[] quad_vertices = {
              1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
             -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
             -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
              1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        };

        uint[] quad_indices = {
            0, 1, 2,
            0, 2, 3
        };

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

    public override void Unload()
    {
        GL.DeleteBuffer(_element_buffer_object);

        base.Unload();
    }
}