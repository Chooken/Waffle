using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;
using WaffleEngine.UI;

namespace OurStory.Scenes;

public class GameScene : IScene
{
    private Window _window;
    private UIToplevel _ui;
    private GpuTexture _source = new ();
    private GpuTexture _destination = new ();
    
    public bool OnSceneLoaded()
    {
        if (!WindowManager.TryOpenWindow("Our Story", "main", 800, 600, out _window))
            return false;
        
        FontLoader.LoadFont("Fonts/Nunito-Regular.ttf", 26);

        _ui = new UIToplevel((WindowSdl)_window);
        _ui.Root = new UIRect()
            .SetWidth(UISize.Percentage(100))
            .SetHeight(UISize.Percentage(100))
            .SetChildAnchor(UIAnchor.Center)
            .AddUIElement(new UIAspectRatio()
                .SetPixelMultiple(new Vector2(256, 256))
                .SetAspectRatio(new Vector2(1, 1))
                .SetWidth(UISize.Percentage(100))
                .SetHeight(UISize.Percentage(100))
                .SetBorderRadius(new Vector4(24, 24, 24, 24), UISizeType.Pixels)
                .SetColor(Color.RGBA255(255, 0, 255, 255))
                .AddUIElement(new UIText()
                    .SetText("Hello World."))
            );

        return true;
    }

    public void OnSceneUpdate()
    {
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(_window, ref _destination);
        _source = _ui.Render(queue);
        queue.AddBlitPass(_source, _destination, true);
        queue.Submit();
    }

    public void OnSceneExit()
    {
        
    }
}