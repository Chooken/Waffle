using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using WaffleEngine.Vector;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Scenes;

public class AssetEditor : INode
{
    public string Path;
    public IVector2 Size;
    public List<Curve> Curves = new List<Curve>();

    public GpuTexture AssetTexture;
    public VectorRenderer Renderer;

    public IVector2 Offset = IVector2.Zero;

    public AssetEditor(IVector2 size)
    {
        Size = size;
        
        AssetTexture = new GpuTexture(GpuTextureSettings.Default((uint)Size.x, (uint)Size.y) with
        {
            ColorTarget = true,
            MagFilter = FilterMode.Nearest,
            MinFilter = FilterMode.Nearest
        });
        Renderer = new VectorRenderer();
        
        Curve curve = new Curve(new Point(){ Position = new Vector2(-0.4f, -0.5f) });
        curve.AddSmooth(new Point(new Vector2(-0.50f, -0.25f)));
        curve.AddSmooth(new Point(new Vector2(0.25f, 0.25f)));
        curve.CloseSmooth();
        
        Curves.Add(curve);

        _selectedCurve = curve;
    }

    public override void OnUpdate()
    {
        foreach (var curve in Curves)
        {
            Renderer.AddCurve(curve);
        }
    }

    private Vector2 grabPosition;
    private IVector2 grabOffset;

    private Curve? _selectedCurve; 

    public override void OnEvent(NodeEvent node_event)
    {
        switch (node_event)
        {
            case NodeEvent.MouseHold:
                if (Input.Mouse.IsLeftPressed)
                {
                    grabPosition = Input.Mouse.Position;
                    grabOffset = Offset;
                } else if (Input.Mouse.IsLeftDown)
                {
                    Vector2 delta = Input.Mouse.Position - grabPosition;
                    Offset = new IVector2(grabOffset.x + (int)delta.x, grabOffset.y + (int)delta.y);
                }
                break;
        }
    }

    public void RenderAsset(ImQueue queue)
    {
        Renderer.Render(queue, AssetTexture);
    }

    private struct TexturedQuad
    {
        public AlignedVector3 Position;
        public Vector2 Size;
        public Vector2 RenderSize;
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        if (!Assets.TryGetShader("builtin", "textured-quad", out var shader))
        {
            throw new NullReferenceException();
        }
        
        renderPass.Bind(shader);
        
        renderPass.Bind(AssetTexture, 0);
        
        Vector2 size = new Vector2((float)Rect.h / 2, (float)Rect.h / 2);
        
        renderPass.SetUniforms(new TexturedQuad
        {
            Position = new AlignedVector3(
                Rect.x + ((float)Rect.w / 2) - (size.x / 2) + Offset.x,
                Rect.y + ((float)Rect.h / 2) - (size.y / 2) + Offset.y,
                0
            ),
            Size = size,
            RenderSize = new Vector2(screenSize.Width, screenSize.Height),
        });
        
        renderPass.DrawPrimatives(6, 1, 0, 0);

        if (_selectedCurve == null)
        {
            return;
        }
        
        if (!Assets.TryGetShader("builtin", "ui-rect", out var handle_shader))
        {
            throw new NullReferenceException();
        }
        
        IRect clip = Tree.Clipstack.TryPeek(out IRect clip_top)
            ? clip_top
            : screenSize;
        
        renderPass.Bind(handle_shader);

        foreach (var point in _selectedCurve.Points)
        {
            renderPass.SetUniforms(new Rect.UIRectData()
            {
                Position = new AlignedVector3(
                    Rect.x + ((float)Rect.w / 2) + Offset.x + point.Position.x * (size.x / 2),
                    Rect.y + ((float)Rect.h / 2) + Offset.y - point.Position.y * (size.y / 2),
                    0
                ),
                Size = new Vector2(10, 10),
                Color = new Vector4(1, 0, 0, 1),
                BorderRadius = new Vector2(5, 5),
                BorderColor = new Vector4(1, 1, 1, 1),
                ScreenSize = new Vector2(screenSize.w, screenSize.h),
                BorderSize = 2.5f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }
    }
}