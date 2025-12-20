using SDL3;
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
    
    public static Color BackgroundColor = Color.RGBA255(25, 25, 25, 255);
    public static Color PanelColor = Color.RGBA255(40, 40, 40, 255);
    public static Color Highlight = Color.RGBA255(0, 255, 132, 255);
    
    public static Color ElementHighlight = Color.RGBA255(60, 60, 60, 255);
    public static Color ElementPressColor = Color.RGBA255(80, 80, 80, 255);

    public static Color FontColor = Color.RGBA255(200, 200, 200, 255);

    public static Font Font;
    
    public static CommandList CommandList = new CommandList();
    public static TextureEditorSharedState SharedState = new ();
    
    public void Start()
    {
        Assert.True(
            WindowManager.TryOpenWindow("Texture Editor", "texture_editor", 800, 600, out EditorWindow),
            "Texture Window Failed to Open.");
        
        Assert.True(
            FontLoader.TryGetFont("builtin/fonts/Nunito-Regular.ttf", 16, out Font), 
            "Failed to load font.");
        
        
        SharedState.SelectedColor = new HSVColor(0,0,1);
        SharedState.Tools.Add(typeof(PenTool), new PenTool());
        SharedState.Tools.Add(typeof(EraserTool), new EraserTool());
        SharedState.Tools.Add(typeof(EyeDropTool), new EyeDropTool());
        SharedState.SelectedTool = SharedState.Tools[typeof(PenTool)];

        _ui = new UiRenderer(EditorWindow, EditorWindow.GetDisplayScale() / EditorWindow.GetDensity());
        _ui.Root = new Rect()
            .Default((ref RectSettings settings) => settings = settings with
            {
                Width = Ui.Grow,
                Height = Ui.Grow,
                Direction = UiDirection.TopToBottom,
                Padding = 8,
                Gap = 8,
            })
            .Add(new Topbar())
            .Add(new Rect()
                .Default((ref RectSettings settings) => settings = settings with
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

        var uiTexture = _ui.Render(queue, BackgroundColor);
        
        queue.AddBlitPass(uiTexture, _swapchainTexture, true);
        queue.Submit();
    }

    public void Close()
    {
        _ui.Dispose();
    }
}