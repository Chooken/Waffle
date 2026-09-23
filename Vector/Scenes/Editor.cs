using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using WaffleEngine.UI.Nodes;
using WaffleEngine.Vector;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Scenes;

public class Editor : IScene
{
    public static Window Window;
    public static GpuTexture SwapchainTexture = new GpuTexture();


    public NodeTree NodeTree;
    public AssetEditor AssetEditor;
    
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
        
        AssetEditor = new AssetEditor(new IVector2(32, 32));

        NodeTree = new NodeTree(AssetEditor);
        
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
        NodeTree.Update(SwapchainTexture);
    }

    private void Render()
    {
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(Window, ref SwapchainTexture);
        
        AssetEditor.RenderAsset(queue);
        
        NodeTree.Draw(queue, SwapchainTexture, true);
        
        queue.Submit();
    }

    public void OnSceneExit()
    {
        Assets.UnloadAssetBundle("Core");
    }
}