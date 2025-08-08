using WaffleEngine.Rendering;

namespace WaffleEngine;

public class Material
{
    private List<Bind> _binds = new List<Bind>();
    private Shader _shader;

    public Material(Shader shader)
    {
        _shader = shader;
    }
    
    public Material(Shader shader, List<Bind> binds)
    {
        _shader = shader;
        _binds = binds;
    }
    
    public void AddTexture(GpuTexture texture, uint slot)
    {
        _binds.Add(new Bind(texture, slot));
    }

    public void AddBuffer<T>(Buffer<T> buffer, uint slot) where T : unmanaged
    {
        _binds.Add(new Bind(buffer, slot));
    }

    public void Bind(IntPtr pass)
    {
        _shader.Bind(pass);
        
        foreach (var bind in _binds)
        {
            bind.Add(pass);
        }
    }
}