using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class TextureEditor
{
    public Window EditorWindow;

    private GpuTexture Canvas;
    private Shader? CanvasBlitShader;
    
    private GpuTexture _swapchainTexture = new GpuTexture();
    private GpuTexture _uiTexture;

    private UiElement Root;

    private ToolPanel _toolPanel = new ToolPanel();
    private ColorPanel _colorPanel = new ColorPanel();
    private CanvasPanel _canvasPanel;
    
    public void Start()
    {
        Assert.True(
            WindowManager.TryOpenWindow("Texture Editor", "texture_editor", 800, 600, out EditorWindow),
            "Texture Window Failed to Open."
        );

        _uiTexture = new GpuTexture(EditorWindow);

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

        EditorWindow.OnWindowResized += OnWindowResized;

        Root = new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Fixed(EditorWindow.Width / EditorWindow.GetDisplayScale()),
                Height = Ui.Fixed(EditorWindow.Height / EditorWindow.GetDisplayScale()),
                Direction = UiDirection.TopToBottom,
                Padding = 8,
                Gap = 8,
            })
            .Add(_toolPanel)
            .Add(new Rect()
                .Default(() => new RectSettings()
                {
                    Width = Ui.Grow,
                    Height = Ui.Grow,
                    Gap = 8,
                })
                .Add(_colorPanel)
                .Add(_canvasPanel)
            );
    }

    public void Update()
    {
        if (!EditorWindow.IsOpen())
            return;
        
        Root.PropagateUpdate(EditorWindow, true);
    }

    public void Render()
    {
        if (!EditorWindow.IsOpen())
            return;
        
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(EditorWindow, ref _swapchainTexture);

        _canvasPanel.RenderCanvas(ref queue);
        
        Ui.RenderToTexture(Root, queue, EditorWindow.GetDisplayScale(), in _uiTexture);
        
        queue.AddBlitPass(_uiTexture, _swapchainTexture, true);
        queue.Submit();
    }
    
    private void OnWindowResized(Vector2 size)
    {
        _uiTexture.Resize((uint)size.x, (uint)size.y);
    }

    public void Close()
    {
        EditorWindow.OnWindowResized -= OnWindowResized;
    }
}