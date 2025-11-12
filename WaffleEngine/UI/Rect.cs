using System.Data;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI.Old;

namespace WaffleEngine.UI;

public class Rect : UiElement
{
    private RectSettings _rectSettings;
    private RectSettings _newRectSettings;
    
    public delegate void ActionRef<T>(ref T item);
    
    private Func<RectSettings>? _default;
    private ActionRef<RectSettings>? _onHoverEvent;
    private ActionRef<RectSettings>? _onClickEvent;
    private ActionRef<RectSettings>? _onHoldEvent;

    public override void Update()
    {
        if (_newRectSettings != _rectSettings)
        {
            _rectSettings.MoveTowards(_newRectSettings);
            Settings = _rectSettings.ToUiSettings();
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

    public override void Render(ImRenderPass renderPass, Vector2 renderSize, float scale)
    {
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
            Position = new AlignedVector3(Bounds.CalulatedPosition * scale),
            Size = new Vector2(Bounds.CalculatedWidth * scale, Bounds.CalculatedHeight * scale),
            Color = _rectSettings.Color,
            BorderRadius = new Vector4(
                _rectSettings.BorderRadius.BottomLeft * scale, 
                _rectSettings.BorderRadius.TopLeft * scale, 
                _rectSettings.BorderRadius.BottomRight * scale, 
                _rectSettings.BorderRadius.TopRight * scale),
            BorderColor = _rectSettings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = _rectSettings.BorderSize * scale,
        };
        
        if (_rectSettings.Color.a != 0)
        {
            renderPass.SetUniforms(data);
            renderPass.Bind(shader);
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        foreach (var child in Children)
        {
            child.Render(renderPass, renderSize, scale);
        }
    }
    
    public Rect Default(Func<RectSettings> defaultSettings)
    {
        _default += defaultSettings;
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