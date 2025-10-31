using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public abstract class Window : IDisposable
{
    public InputHandler WindowInput = new InputHandler();
    
    public string WindowHandle { get; internal set; }
    
    public int Width { get; internal set; }
    
    public int Height { get; internal set; }
    
    public bool Resizeable { get; protected set; }

    public abstract uint GetId();
        
    public abstract void SetSize(int width, int height);

    public abstract void Focus();

    public abstract void SetTitle(string title);

    public abstract float GetDisplayScale();

    public abstract bool TryGetSwapchainTexture(ImQueue queue, ref GpuTexture texture);

    public abstract TextureFormat GetSwapchainTextureFormat();
    
    public abstract void Dispose();

    public abstract bool IsOpen();
}