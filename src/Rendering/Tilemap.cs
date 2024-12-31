
using OpenTK.Graphics.OpenGL;
using System.Numerics;

namespace WaffleEngine;

public class Tilemap
{
    private Shader _shader;
    private DefaultTilemapMaterial _material;
    private TilemapMesh _mesh;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Tilemap(Texture texture, Vector2 position, int width, int height)
    {
        Width = width;
        Height = height;

        _shader = Shader.Get("core", "tilemap");
        _material = new DefaultTilemapMaterial(_shader, texture, width * height);
        _mesh = TilemapMesh.Create(_material, position, width, height);

        _material.SetTile(1, 2);
    }

    public void Render(Camera camera)
    {
        _mesh.Bind();

        _material.Enable(camera);

        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        _material.Disable();
    }

    public void Dispose()
    {
        _material.Dispose();
        _mesh.Unload();
    }
}