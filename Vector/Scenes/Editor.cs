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

        Renderer = new VectorRenderer();
        
        curve.AddPoint(new Point(){ Position = new Vector2( -0.5f, -0.25f ) });
        curve.AddPoint(new Point(){ Position = new Vector2( 0.75f, 0.5f ) });
        
        return true;
    }

    public void OnSceneUpdate()
    {
        Update();
        Render();
    }

    private Curve curve = new Curve();

    private void Update()
    {
        Renderer.AddCurve(curve);
    }

    private void Render()
    {
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(Window, ref SwapchainTexture);
        
        Renderer.Render(queue, SwapchainTexture);
        
        queue.Submit();
    }

    public void OnSceneExit()
    {
        Assets.UnloadAssetBundle("Core");
    }
}