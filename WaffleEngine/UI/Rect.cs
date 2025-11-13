using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class Rect : UiElement
{
    protected RectSettings RectSettings;
    private RectSettings _newRectSettings;

    private Shader? _shader;
    
    public delegate void ActionRef<T>(ref T item);
    
    private Func<RectSettings>? _default;
    private ActionRef<RectSettings>? _onHoverEvent;
    private ActionRef<RectSettings>? _onClickEvent;
    private ActionRef<RectSettings>? _onHoldEvent;
    
    public struct UIRectData
    {
        public AlignedVector3 Position;
        public Vector2 Size;
        public Vector4 Color;
        public Vector4 BorderRadius;
        public Vector4 BorderColor;
        public Vector2 ScreenSize;
        public float BorderSize;
    }
    
    private bool SetupShader()
    {
        if (!Assets.TryGetShader("Core", "crt", out _shader))
        {
            Log.Error("Shader not found");
            return false;
        }
        
        _shader.SetPipeline(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.One,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
            VertexAttributes = null,
        });

        return true;
    }

    public override void Update()
    {
        if (_newRectSettings != RectSettings)
        {
            RectSettings.MoveTowards(_newRectSettings);
            Settings = RectSettings.ToUiSettings();
        }

        _newRectSettings = _default?.Invoke() ?? default;
    }
    
    public override void OnHover()
    {
        _onHoverEvent?.Invoke(ref _newRectSettings);
    }

    public override void OnClick()
    {
        _onClickEvent?.Invoke(ref _newRectSettings);
    }

    public override void OnHold()
    {
        _onHoldEvent?.Invoke(ref _newRectSettings);
    }

    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        if (_shader is null)
        {
            if (SetupShader())
            {
                return;
            }
        }
        
        if (!Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            WLog.Error("Shader not found");
            return;
        }
        
        shader.SetPipeline(new PipelineSettings
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.One,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
            VertexAttributes = null,
        });
        
        UIRectData data = new UIRectData()
        {
            Position = new AlignedVector3(Bounds.CalculatedPosition),
            Size = new Vector2(Bounds.CalculatedWidth, Bounds.CalculatedHeight),
            Color = RectSettings.Color,
            BorderRadius = new Vector4(
                RectSettings.BorderRadius.BottomLeft, 
                RectSettings.BorderRadius.TopLeft, 
                RectSettings.BorderRadius.BottomRight, 
                RectSettings.BorderRadius.TopRight),
            BorderColor = RectSettings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = RectSettings.BorderSize,
        };
        
        if (RectSettings.Color.a != 0)
        {
            renderPass.SetUniforms(data);
            renderPass.Bind(shader);
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }
    }
    
    public Rect Default(Func<RectSettings> defaultSettings)
    {
        _default += defaultSettings;
        RectSettings = _default.Invoke();
        _newRectSettings = RectSettings;
        Settings = RectSettings.ToUiSettings();
        return this;
    }

    public Rect OnHover(ActionRef<RectSettings> hover)
    {
        _onHoverEvent += hover;
        return this;
    }

    public Rect OnClick(ActionRef<RectSettings> click)
    {
        _onClickEvent += click;
        return this;
    }
    
    public Rect OnHold(ActionRef<RectSettings> hold)
    {
        _onHoldEvent += hold;
        return this;
    }

    public Rect Add(UiElement child)
    {
        Children.Add(child);
        child.Parent = this;
        return this;
    }
}