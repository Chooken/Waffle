using OurStory.Editor.UI_Components;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public static class Editor
{
    private static Window? window;
    
    public static Color BackgroundColor = Color.RGBA255(25, 25, 25, 255);
    public static Color PanelColor = Color.RGBA255(40, 40, 40, 255);
    public static Color Highlight = Color.RGBA255(0, 255, 132, 255);
    
    public static Color ElementHighlight = Color.RGBA255(60, 60, 60, 255);
    public static Color ElementPressColor = Color.RGBA255(80, 80, 80, 255);

    public static Color FontColor = Color.RGBA255(200, 200, 200, 255);
    
    public static UiRenderer UiRenderer;
    public static GpuTexture SwapchainTexture = new ();
    
    public static bool Open()
    {
        if (window is not null)
        {
            window.Focus();
            return true;
        }

        if (!WindowManager.TryOpenWindow("Editor", "Editor", 800, 600, out window))
        {
            return false;
        }

        UiRenderer = new UiRenderer(window, window.GetDisplayScale() / window.GetDensity());
        UiRenderer.Root = new EditorUI();

        return true;
    }

    public static void Update()
    {
        UiRenderer.UpdateUi();

        ImQueue queue = new ImQueue();
        
        if (!queue.TryGetSwapchainTexture(window, ref SwapchainTexture))
        {
            queue.Submit();
            return;
        }
        
        var uiTexture = UiRenderer.Render(queue, BackgroundColor);
        
        queue.AddBlitPass(uiTexture, SwapchainTexture, true);
        queue.Submit();
    }

    public static void Close()
    {
        WindowManager.CloseWindow("Editor");
        window = null;
    }
}