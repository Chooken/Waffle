using System.Numerics;
using System.Text.Json;
using WaffleEngine.UI;

namespace WaffleEngine.Rendering;

public class Renderer
{
    private WindowSdl _window;
    
    private Shader _shader;
    private Queue _queue;
    private GpuTexture _renderTexture = new();

    private Buffer<Vertex> vertices;
    private Buffer<int> indices;

    private Texture texture;
    private GpuTexture text;

    private ValueBox<uint> instances = new ValueBox<uint>(1);

    private UIToplevel ui;
    
    public Renderer(WindowSdl window)
    {
        _window = window;
        _queue = new Queue();
        
        texture = new Texture("textures/texture.png");
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/triangle", out _shader))
        {
            Log.Error("Shader not found");
            return;
        }

        Mesh mesh = Mesh.Quad(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0));
        
        //Upload Buffer to GPU
        var copyPass = new CopyPass();
        copyPass.AddCommand(new UploadToGpu(mesh));
        copyPass.AddCommand(new UploadToGpu(texture));
        _queue.AddPass(copyPass);
        _queue.Submit();
        _queue.Clear();
        
        //Create the render Queue

        text = new GpuTexture(window);
        
        Material material = new Material(_shader);
        material.AddTexture(texture, 0);
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = Color.RGBA255(49, 54, 63, 255),
            GpuTexture = text,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };
        
        _queue.AddPreprocess(new GetSwapchain(_window, ref _renderTexture));
        RenderPass renderPass = new RenderPass(colorTargetSettings);
        renderPass.AddCommand(new Bind(material));
        renderPass.AddCommand(new Bind(mesh));
        renderPass.AddCommand(new DrawIndexedPrimatives(6, instances, 0, 0, 0));
        _queue.AddPass(renderPass);

        ui = new UIToplevel(window);
        ui.BackgroundColor = Color.RGBA255(34, 40, 49, 255);
        ui.Root = new UIRect()
            .SetWidth(UISize.Percentage(100))
            .SetHeight(UISize.Percentage(100))
            .SetChildDirection(UIDirection.None)
            .AddUIElement(new UIRect()
                .SetWidth(UISize.Percentage(100))
                .SetHeight(UISize.Percentage(100))
                .SetPaddingX(UISize.Pixels(12))
                .SetPaddingY(UISize.Pixels(12))
                .AddUIElement(new UIRect()
                    .SetWidth(UISize.Percentage(20))
                    .SetHeight(UISize.Percentage(100))
                    .SetChildDirection(UIDirection.Down)
                    .SetChildAnchor(UIAnchor.Center)
                    .AddUIElement(new UIRect()
                        .SetWidth(UISize.Percentage(100))
                        .SetHeight(UISize.Percentage(50))
                        .AddUIElement(new UIRect()
                            .SetColor(Color.RGBA255(255, 40, 49, 255))
                            .SetWidth(UISize.Percentage(50))
                            .SetHeight(UISize.Percentage(100))
                            .SetMarginX(UISize.Pixels(12))
                            .SetMarginY(UISize.Pixels(12))
                            .SetBorderRadius(new Vector4(25f, 25f, 25f, 25f), UISizeType.Pixels)
                        )
                        .AddUIElement(new UIRect()
                            .SetColor(Color.RGBA255(255, 120, 49, 255))
                            .SetWidth(UISize.Percentage(50))
                            .SetHeight(UISize.Percentage(100))
                            .SetMarginX(UISize.Pixels(12))
                            .SetMarginY(UISize.Pixels(12))
                            .SetBorderRadius(new Vector4(25f, 25f, 25f, 25f), UISizeType.Pixels)
                            // .SetPaddingX(UISize.Pixels(12))
                            // .SetPaddingY(UISize.Pixels(12))
                            .AddUIElement(new UIText()
                                .SetText("Hello World.")
                                .SetTextColor(Color.RGBA255(255,255,255,255))
                                .SetMarginX(UISize.Pixels(12))
                                .SetMarginY(UISize.Pixels(12))
                            )
                        )
                    )
                    .AddUIElement(new UIRect()
                        .SetColor(Color.RGBA255(34, 255, 49, 255))
                        .SetWidth(UISize.Percentage(100))
                        .SetHeight(UISize.Percentage(50))
                        .SetMarginX(UISize.Pixels(12))
                        .SetMarginY(UISize.Pixels(12))
                        .SetBorderRadius(new Vector4(25f, 25f, 25f, 25f), UISizeType.Pixels)
                    )
                )
                .AddUIElement(new UIRect()
                    .SetWidth(UISize.Percentage(80))
                    .SetHeight(UISize.Percentage(100))
                    .SetMarginX(UISize.Pixels(12))
                    .SetMarginY(UISize.Pixels(12))
                    .SetBorderRadius(new Vector4(25f, 25f, 25f, 25f), UISizeType.Pixels)
                    .SetTexture(text)
                )
            )
            .AddUIElement(new UIRect()
                .SetWidth(UISize.Percentage(100))
                .SetHeight(UISize.Percentage(100))
                .SetChildAnchor(UIAnchor.Center)
            );
        
        _queue.AddPass(new UIPass(ui));
        
        BlitPass blitPass2 = new BlitPass(ui.UiTexture, _renderTexture, true);
        _queue.AddPass(blitPass2);
    }
    
    internal void Render()
    {
        // instances.SetValue(instances + 1);
        _queue.Submit();
    }
}