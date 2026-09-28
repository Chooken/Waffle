using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using WaffleEngine.Vector;

namespace Vector.Scenes;

public class Game : IScene
{
    public static Window Window;
    public static GpuTexture SwapchainTexture = new GpuTexture();

    public World World = new World();
    
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
        
        return true;
    }

    public void OnSceneUpdate()
    {
        ImQueue queue = new ImQueue();
        Update(queue);
        queue.TryGetSwapchainTexture(Window, ref SwapchainTexture);
        Render(queue, SwapchainTexture);
        queue.Submit();
    }
    
    private void Update(ImQueue queue)
    {
        World.Update(queue);
    }

    private void Render(ImQueue queue, GpuTexture target)
    {
        World.Render(queue, target);
    }

    public void OnSceneExit()
    {
        Assets.UnloadAssetBundle("Core");
    }
}