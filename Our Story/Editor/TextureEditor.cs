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

    private struct CanvasBlitUniforms
    {
        public Color SelectedColor;
        public Vector2 CursorPosition;
        public Vector2 TextureSize;
    }
    
    private GpuTexture _swapchainTexture = new GpuTexture();
    
    private UIToplevel _ui;
    private Color _selectedColor = Color.RGBA255(255,255,255,255);
    private Vector2 _cursorPosition = Vector2.Zero;
    
    public void Start()
    {
        Assert.True(
            WindowManager.TryOpenWindow("Texture Editor", "texture_editor", 800, 600, out EditorWindow),
            "Texture Window Failed to Open."
        );

        Assert.True(
            Assets.TryGetTexture("Core", "Character_hat", out Texture),
            "Base Texture not found."
        );
        
        Assert.True(
            Assets.TryGetShader("Core", "texture_canvas", out CanvasBlitShader),
            "Canvas Blit Shader not found."
        );
        
        CanvasBlitShader.SetPipeline(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
            VertexAttributes = null
        });

        Canvas = new GpuTexture(GpuTextureSettings.Default((uint)Texture.Width, (uint)Texture.Height) with
        {
            ColorTarget = true,
        });
        
        var colorPanel = new UIRect()
            .Default(() => new UISettings()
            {
                Width = UISize.PercentageWidth(30),
                Height = UISize.PercentageHeight(100),
                Color = Color.RGBA255(22, 22, 22, 255),
                ChildDirection = UIDirection.Down,
                PaddingX = UISize.Pixels(8),
                PaddingY = UISize.Pixels(8),
                BorderRadius = new UIBorderRadius(20, 20, 20, 20, UISizeType.Pixels)
            });

        int rows = 17;
        int columns = 7;
        
        for (int y = 0; y < rows - 1; y++)
        {
            var rect = new UIRect()
                .Default(() => new UISettings()
                {
                    Width = UISize.PercentageWidth(100),
                    Height = UISize.PercentageHeight(100f / rows),
                    Gap = UISize.Pixels(4),
                    PaddingY = UISize.Pixels(2),
                });
            
            for (int x = 0; x < columns; x++)
            {
                OklabColor color = OklabColor.FromLCH((1f - (float)x / columns) * 0.7f + 0.3f, (1f - (float)x / columns) * 0.125f, (float)y / rows * Single.Pi * 2);
                
                AddColorButton(rect, columns, color);
            }

            colorPanel.AddUIElement(rect);
        }
        
        var finalRect = new UIRect()
            .Default(() => new UISettings()
            {
                Width = UISize.PercentageWidth(100),
                Height = UISize.PercentageHeight(100f / rows),
                Gap = UISize.Pixels(4),
                PaddingY = UISize.Pixels(2),
            });
        
        AddColorButton(finalRect, columns, new Color(1,1,1,1));
        AddColorButton(finalRect, columns, new Color(0,0,0,1));
        AddColorButton(finalRect, columns, new Color(0,0,0,0));

        colorPanel.AddUIElement(finalRect);

        UICrt crt = new UICrt(new Vector2(Texture.Width, Texture.Height), 0.25f);

        _ui = new UIToplevel((WindowSdl)EditorWindow);
        _ui.Root = new UIRect()
            .Default(() => new UISettings()
            {
                Width = UISize.PercentageWidth(100),
                Height = UISize.PercentageHeight(100),
                ChildAnchor = UIAnchor.TopLeft,
                PaddingX = UISize.Pixels(8),
                PaddingY = UISize.Pixels(8),
                Gap = UISize.Pixels(8),
            })
            .AddUIElement(colorPanel)
            .AddUIElement(new UIRect()
                .Default(() => new UISettings()
                {
                    Width = UISize.PercentageWidth(70),
                    Height = UISize.PercentageHeight(100),
                    ChildAnchor = UIAnchor.Center
                })
                .AddUIElement(crt
                    .Default(() => new UISettings()
                    {
                        Width = UISize.PercentageWidth(100),
                        Height = UISize.PercentageHeight(100),
                        BorderRadius = new UIBorderRadius(5, 5, 5, 5, UISizeType.PercentageWidth),
                        BorderSize = UISize.Pixels(4),
                        BorderColor = Color.RGBA255(22, 22, 22, 255),
                        Texture = Canvas,
                    })
                    .OnHover((ref UISettings settings) =>
                    {
                        Vector2 position = EditorWindow.WindowInput.MouseData.Position;
                        Vector3 uiPos = crt.LastPosition;
                        Vector2 uiSize = crt.LastSize;

                        _cursorPosition = new Vector2(
                            (int)((position.x - uiPos.x) / uiSize.x * crt.Resolution.x), 
                            (int)((position.y - uiPos.y) / uiSize.y * crt.Resolution.y));
                    })
                    .OnHold((ref UISettings settings) =>
                    {
                        Vector2 position = EditorWindow.WindowInput.MouseData.Position;
                        Vector3 uiPos = crt.LastPosition;
                        Vector2 uiSize = crt.LastSize;

                        Vector2 texturePos = new Vector2(
                            (position.x - uiPos.x) / uiSize.x * crt.Resolution.x, 
                            (position.y - uiPos.y) / uiSize.y * crt.Resolution.y);
                        
                        Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4] = _selectedColor.r255;
                        Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4 + 1] = _selectedColor.g255;
                        Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4 + 2] = _selectedColor.b255;
                        Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4 + 3] = _selectedColor.a255;

                        ImQueue queue = new ImQueue();
                        ImCopyPass copyPass = queue.AddCopyPass();
                        copyPass.Upload(Texture);
                        copyPass.End();
                        queue.Submit();
                    })
                )
            );
    }

    private void AddColorButton(UIRect rect, int columns, Color color)
    {
        rect.AddUIElement(new UIRect()
            .Default(() => new UISettings()
            {
                Width = UISize.PercentageWidth(100f / columns),
                Height = UISize.PercentageHeight(100f),
                Color = color.WithAlphaOne(),
                BorderRadius = new UIBorderRadius(20, 20, 20, 20, UISizeType.PercentageWidth),
                BorderSize = UISize.Pixels(color == _selectedColor ? 4 : 0),
                BorderColor = new Color(1, 1, 1, 1),
            })
            .OnHover((ref UISettings settings) =>
            {
                if (color == _selectedColor)
                    return;
                        
                settings.BorderSize = UISize.Pixels(4);
                settings.BorderColor = Color.RGBA255(0,0,0,255);
            })
            .OnClick((ref UISettings settings) =>
            {
                _selectedColor = color;
            })
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

        ImRenderPass renderPass = queue.AddRenderPass(new ColorTargetSettings()
        {
            GpuTexture = Canvas,
            ClearColor = Color.RGBA255(24, 24, 24, 255),
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        });
        
        renderPass.Bind(CanvasBlitShader);
        renderPass.Bind(Texture);
        renderPass.SetUniforms(new CanvasBlitUniforms
        {
            SelectedColor = _selectedColor,
            CursorPosition = _cursorPosition,
            TextureSize = new Vector2(Texture.Width, Texture.Height),
        });
        renderPass.DrawPrimatives(3, 1, 0, 0);
        renderPass.End();
        
        GpuTexture _uiTexture = _ui.Render(queue);
        queue.AddBlitPass(_uiTexture, _swapchainTexture, true);
        queue.Submit();
    }
}