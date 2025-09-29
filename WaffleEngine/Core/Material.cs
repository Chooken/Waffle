using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SDL3;
using WaffleEngine.Native;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public class Material : IGpuBindable
{
    private List<(IGpuBindable, uint)> _binds = new List<(IGpuBindable, uint)>();
    
    private Shader _shader;

    public Material(Shader shader)
    {
        _shader = shader;
    }

    public void Clear()
    {
        _binds.Clear();
    }
    
    public void AddTexture(GpuTexture texture, uint slot)
    {
        _binds.Add((texture, slot));
    }

    public void AddBuffer<T1>(Buffer<T1> buffer, uint slot) where T1 : unmanaged
    {
        _binds.Add((buffer, slot));
    }

    public void Bind(ImRenderPass pass, uint _ = 0)
    {
        _shader.Bind(pass);
        
        foreach (var bind in _binds)
        {
            bind.Item1.Bind(pass, bind.Item2);
        }
    }
}