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

    public void AddTexture(Texture texture, uint slot)
    {
        _shader._samplerCount++;
        
        _binds.Add(new Bind(texture, slot));
    }

    public void AddBuffer<T>(Buffer<T> buffer, uint slot) where T : unmanaged
    {
        switch (buffer.Usage)
        {
            case BufferUsage.GraphicsStorageRead:
                _shader._storageBufferCount++;
                break;
        }
        
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