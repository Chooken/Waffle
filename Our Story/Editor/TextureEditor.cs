using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class TextureEditor
{
    public Window EditorWindow;
    
    private GpuTexture _swapchainTexture = new GpuTexture();
    private UiRenderer _ui;
    private Canvas _canvas = new Canvas(16, 16);
    
    public static Color BackgroundColor = Color.RGBA255(20, 20, 20, 255);
    public static Color PanelColor = Color.RGBA255(30, 30, 30, 255);
    public static Color ElementColor = Color.RGBA255(40, 40, 40, 255);
    public static Color ElementHighlight = Color.RGBA255(0, 255, 132, 255);
    public static Color ElementPressColor = Color.RGBA255(25, 25, 25, 255);
    
    public static CommandList CommandList = new CommandList();
    public static TextureEditorSharedState SharedState = new ();
    
    public void Start()
    {
        Assert.True(
            WindowManager.TryOpenWindow("Texture Editor", "texture_editor", 800, 600, out EditorWindow),
            "Texture Window Failed to Open.");
        
        Assert.True(
            FontLoader.TryGetFont("builtin/fonts/Nunito-Regular.ttf", 16, out Font font), 
            "Failed to load font.");
        
        
        SharedState.SelectedColor = new Color(1, 1, 1, 1);
        SharedState.ColorBrightness = 1;
        SharedState.Tools.Add(typeof(PenTool), new PenTool());
        SharedState.Tools.Add(typeof(EraserTool), new EraserTool());
        SharedState.SelectedTool = SharedState.Tools[typeof(PenTool)];

        _ui = new UiRenderer(EditorWindow, EditorWindow.GetDisplayScale() / EditorWindow.GetDensity());
        _ui.Root = new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Grow,
                Height = Ui.Grow,
                Direction = UiDirection.TopToBottom,
                Padding = 8,
                Gap = 8,
            })
            .Add(new ToolPanel(font))
            .Add(new Rect()
                .Default(() => new RectSettings()
                {
                    Width = Ui.Grow,
                    Height = Ui.Grow,
                    Gap = 8,
                })
                .Add(new ColorPanel())
                .Add(new CanvasPanel(EditorWindow, _canvas))
            );
        
        _ui.SetEnforceWindowSize(true);
    }

    public void Update()
    {
        if (!EditorWindow.IsOpen())
            return;
        
        _ui.UpdateUi();
    }

    public void Render()
    {
        if (!EditorWindow.IsOpen())
            return;
        
        ImQueue queue = new ImQueue();
        queue.TryGetSwapchainTexture(EditorWindow, ref _swapchainTexture);
        
        _canvas.Render(ref queue);

        var uiTexture = _ui.Render(queue);
        
        queue.AddBlitPass(uiTexture, _swapchainTexture, true);
        queue.Submit();
    }

    public void Close()
    {
        _ui.Dispose();
    }
}