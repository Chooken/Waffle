using Vector.Editor.UI;
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
    public Toolbar Toolbar;
    public ShapeListPanel ShapeList;
    public InspectorPanel Inspector;
    public StatusBar Status;
    public SettingsPopup Popup;
    
    public bool OnSceneLoaded()
    {
        if (!Assets.TryLoadAssetBundle("Core"))
        {
            return false;
        }

        if (!WindowManager.TryOpenMainWindow("Vector", 1280, 800, out Window))
        {
            return false;
        }
        
        Application.SetUpdateRate(60);
        
        AssetEditor = new AssetEditor(new IVector2(32, 32));
        Toolbar = new Toolbar { Editor = AssetEditor };
        ShapeList = new ShapeListPanel { Editor = AssetEditor };
        Inspector = new InspectorPanel { Editor = AssetEditor };
        Status = new StatusBar { Editor = AssetEditor };
        Popup = new SettingsPopup { Editor = AssetEditor };
        AssetEditor.Popup = Popup;

        // Popup first so the modal wins mouse events over everything.
        Rect root = new Rect();
        NodeTree = new NodeTree(root);
        root.AddNode(Popup);
        EditorLayout layout = new();
        
        INode topbar_splitview = root.AddNode(new FixedSplitView(Theme.ToolbarHeight, true, false));
        topbar_splitview.AddNode(Toolbar);

        INode bottombar_splitview = topbar_splitview.AddNode(new FixedSplitView(Theme.StatusHeight, false, false));
        bottombar_splitview.AddNode(Status);
        
        INode shapelist_splitview = bottombar_splitview.AddNode(new SplitView(Theme.SideWidth, 12));
        shapelist_splitview.AddNode(ShapeList);

        INode inspector_splitview = shapelist_splitview.AddNode(new SplitView(Theme.SideWidth, 12));
        inspector_splitview.AddNode(AssetEditor);
        inspector_splitview.AddNode(Inspector);
        
        NodeTree.SetClearColor(Theme.Bg);
        
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
        // Live so theme switches re-tint the background immediately.
        NodeTree.SetClearColor(Theme.Bg);
        NodeTree.Update(SwapchainTexture);
    }

    private void Render()
    {
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(Window, ref SwapchainTexture);

        AssetEditor.RenderAsset(queue);

        // The modal draws in an overlay pass so it always lands on top of
        // the asset; event priority is separate (popup is the root's first
        // child, so it still wins mouse while open).
        Popup.SetEnabled(false);
        NodeTree.Draw(queue, SwapchainTexture, true);
        if (Popup.IsOpen)
        {
            Popup.SetEnabled(true);
            var overlay = queue.AddRenderPass(new ColorTargetSettings
            {
                ClearColor = new Color(0, 0, 0, 0),
                GpuTexture = SwapchainTexture,
                LoadOperation = LoadOperation.Load,
                StoreOperation = StoreOperation.Store,
            });
            Popup.Draw(overlay, NodeTree.UnitRect);
            overlay.End();
            Popup.SetEnabled(false);
        }
        Popup.SetEnabled(Popup.IsOpen);

        queue.Submit();
    }

    public void OnSceneExit()
    {
        Assets.UnloadAssetBundle("Core");
    }
}