using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine;

public static class FullscreenRenderer
{
    private static Dictionary<Material, FullscreenMesh> _cached_meshs = new();

    public static unsafe void Render(Material material)
    {
        if (!_cached_meshs.ContainsKey(material))
        {
            _cached_meshs[material] = FullscreenMesh.Create(material);
        }

        FullscreenMesh mesh = _cached_meshs[material];

        mesh.Bind();
        mesh.Material.Enable(null);

        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);

        mesh.Material.Disable();
    }
}
