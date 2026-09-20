using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Vector;

namespace Vector.Scenes;

public class Editor : IScene
{
    public static Window Window;
    public static GpuTexture SwapchainTexture = new GpuTexture();

    public VectorRenderer Renderer;
    public DownscaledTexture Texture = new DownscaledTexture(DownscaledTexture.ScaleMode.Height, 108);
    
    public bool OnSceneLoaded()
    {
        if (!Assets.TryLoadAssetBundle("Core"))
        {
            return false;
        }

        if (!WindowManager.TryOpenMainWindow("Editor", 800, 600, out Window))
        {
            return false;
        }
        
        Application.SetUpdateRate(60);

        Renderer = new VectorRenderer();
        
        curve.AddSmooth(new Point(new Vector2(-0.50f, -0.25f)));
        curve.AddSmooth(new Point(new Vector2(0.25f, 0.25f)));
        curve.CloseSmooth();
        
        return true;
    }

    public void OnSceneUpdate()
    {
        Update();
        Render();
    }

    private Curve curve = new Curve(new Point(){ Position = new Vector2(-0.4f, -0.5f) });

    private void Update()
    {
        Renderer.AddCurve(curve);
    }

    private void Render()
    {
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(Window, ref SwapchainTexture);
        
        Texture.SetOutputTexture(SwapchainTexture);
        
        Renderer.Render(queue, Texture.GetTexture());

        Texture.GetFullResTexture(queue);
        
        queue.Submit();
    }

    public void OnSceneExit()
    {
        Assets.UnloadAssetBundle("Core");
    }
}