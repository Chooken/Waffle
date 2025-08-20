namespace WaffleEngine;

public abstract class Window : IDisposable
{
    public InputHandler WindowInput = new InputHandler();
    
    public int Width { get; internal set; }
    
    public int Height { get; internal set; }

    public abstract uint GetId();
        
    public abstract void SetSize(int width, int height);

    public abstract void Focus();
    
    public abstract void Dispose();
}