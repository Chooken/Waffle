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

    public VectorRenderer Renderer;
    public DownscaledTexture Texture = new DownscaledTexture(DownscaledTexture.ScaleMode.Height, 108);
    public NodeTree NodeTree;
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

        NodeTree = new NodeTree(new FixedView()
        {
            VerticalAlignment = FixedView.Alignment.Middle,
            HorizontalAlignment = FixedView.Alignment.Middle,
            Size = new IVector2(10, 10),
        });
        
        NodeTree.SetHeightInUnits(108);

        NodeTree.Root.AddNode(new Rect
        {
            Color = Color.RGBA255(255, 0, 0, 255),
            BorderRadius = Vector4.One,
        });
        
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
        
        NodeTree.Update(SwapchainTexture);
    }

    private void Render()
    {
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(Window, ref SwapchainTexture);
        
        Texture.SetOutputTexture(SwapchainTexture);
        
        Renderer.Render(queue, Texture.GetTexture());

        Texture.GetFullResTexture(queue);
        
        NodeTree.Draw(queue, SwapchainTexture);
        
        queue.Submit();
    }

    public void OnSceneExit()
    {
        Assets.UnloadAssetBundle("Core");
    }
}