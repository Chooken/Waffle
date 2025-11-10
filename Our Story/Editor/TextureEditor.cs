using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class TextureEditor
{
    public Window EditorWindow;
    public Texture Texture;

    private GpuTexture Canvas;
    private Shader? CanvasBlitShader;
    
    private GpuTexture _swapchainTexture = new GpuTexture();
    
    private UIToplevel _ui;
    private Vector2 _cursorPosition = Vector2.Zero;

    private ToolPanel _toolPanel = new ToolPanel();
    private ColorPanel _colorPanel = new ColorPanel();
    private CanvasPanel _canvasPanel;
    
    public void Start()
    {
        Assert.True(
            WindowManager.TryOpenWindow("Texture Editor", "texture_editor", 800, 600, out EditorWindow),
            "Texture Window Failed to Open."
        );

        _canvasPanel = new CanvasPanel(EditorWindow, 16, 16);
        _canvasPanel.CanvasTool = new PenTool();
        _colorPanel.OnColorSelected += color =>
        {
            _canvasPanel.CursorColor = color;
        };
        _toolPanel.OnToolSelected += tool =>
        {
            _canvasPanel.CanvasTool = tool;
        };

        _ui = new UIToplevel((WindowSdl)EditorWindow);
        _ui.Root = new UIRect()
            .Default(() => new UISettings()
            {
                Grow = true,
                ChildDirection = UIDirection.Down,
                PaddingX = UISize.Pixels(8),
                PaddingY = UISize.Pixels(8),
                Gap = UISize.Pixels(8),
            })
            .AddUIElement(_toolPanel)
            .AddUIElement(new UIRect()
                .Default(() => new UISettings()
                {
                    Grow = true,
                    Width = UISize.PercentageWidth(100),
                    Gap = UISize.Pixels(8),
                })
                .AddUIElement(_colorPanel)
                .AddUIElement(_canvasPanel)
            );
    }

    public void Update()
    {
        if (!EditorWindow.IsOpen())
            return;
        
        _ui.Update();
    }

    public void Render()
    {
        if (!EditorWindow.IsOpen())
            return;
        
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(EditorWindow, ref _swapchainTexture);

        _canvasPanel.RenderCanvas(ref queue);
        
        GpuTexture _uiTexture = _ui.Render(queue);
        queue.AddBlitPass(_uiTexture, _swapchainTexture, true);
        queue.Submit();
    }
}