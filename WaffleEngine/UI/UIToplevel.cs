using System.Numerics;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public struct UIToplevel
{
    public readonly Window Window;
    public UIRect? Root;
    public Color BackgroundColor = Color.RGBA255(0,0,0,0);
    
    private Vector2 _screenSize;
    
    public GpuTexture UiTexture;

    public UIToplevel(Window window)
    {
        Window = window;
        UiTexture = new GpuTexture(Window);
        _screenSize = new Vector2(window.Width, window.Height);
    }

    public void Update()
    {
        Root?.PropagateUpdate(Window, true);
    }

    public GpuTexture Render(ImQueue queue)
    {
        if (Root is null)
            return UiTexture;
        
        UISize.SetScale(Window.GetDisplayScale());

        bool resized = _screenSize != new Vector2(Window.Width, Window.Height);
        
        if (resized)
        {
            UiTexture.Resize((uint)Window.Width, (uint)Window.Height);
        }

        if (Root.Dirty || resized)
        {
            ColorTargetSettings bgColorTargetSettings = new ColorTargetSettings
            {
                ClearColor = BackgroundColor,
                GpuTexture = UiTexture,
                LoadOperation = LoadOperation.Clear,
                StoreOperation = StoreOperation.Store,
            };

            var renderPass = queue.AddRenderPass(bgColorTargetSettings);
            
            Root.Render(
                queue,
                renderPass,
                new Vector3(0, 0, 0), 
                new Vector2(UiTexture.Width, UiTexture.Height), 
                Vector2.Zero, 
                new Vector2(UiTexture.Width, UiTexture.Height));
            
            renderPass.End();
        }

        _screenSize = new Vector2(UiTexture.Width, UiTexture.Height);

        return UiTexture;
    }

    public UIToplevel SetRoot(UIRect uiRect)
    {
        Root = uiRect;
        return this;
    }
}