using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class Rect : UiElement
{
    protected RectSettings RectSettings;
    private RectSettings _newRectSettings;

    private Shader? _shader;
    
    public delegate void ActionRef<T>(ref T item);
    
    private ActionRef<RectSettings>? _default;
    private ActionRef<RectSettings>? _onHoverEvent;
    private ActionRef<RectSettings>? _onMouseDownEvent;
    private ActionRef<RectSettings>? _onHoldEvent;
    private ActionRef<RectSettings>? _onMouseUpEvent;
    
    public struct UIRectData
    {
        public AlignedVector3 Position;
        public Vector2 Size;
        public Vector4 TopLeftColor;
        public Vector4 TopRightColor;
        public Vector4 BottomLeftColor;
        public Vector4 BottomRightColor;
        public Vector4 BorderRadius;
        public Vector4 BorderColor;
        public Vector2 ScreenSize;
        public float BorderSize;
    }
    
    private bool SetupShader()
    {
        if (!Assets.TryGetShader("builtin", "ui-rect", out _shader))
        {
            Log.Error("Shader not found");
            return false;
        }

        return true;
    }

    public override void Update()
    {
        if (_newRectSettings != RectSettings || Settings != RectSettings.ToUiSettings())
        {
            RectSettings.MoveTowards(_newRectSettings);
            Settings = RectSettings.ToUiSettings();
        }

        _newRectSettings = new RectSettings();

        _default?.Invoke(ref _newRectSettings);
    }
    
    public override bool OnHover()
    {
        if (_onHoverEvent is null)
            return false;
        
        _onHoverEvent.Invoke(ref _newRectSettings);
        return _newRectSettings.CaptureInput;
    }

    public override bool OnMouseDown()
    {
        if (_onMouseDownEvent is null)
            return false;
        
        _onMouseDownEvent.Invoke(ref _newRectSettings);
        return _newRectSettings.CaptureInput;
    }

    public override bool OnHold()
    {
        if (_onHoldEvent is null)
            return false;
        
        _onHoldEvent.Invoke(ref _newRectSettings);
        return _newRectSettings.CaptureInput;
    }

    public override bool OnMouseUp()
    {
        if (_onMouseUpEvent is null)
            return false;
        
        _onMouseUpEvent.Invoke(ref _newRectSettings);
        return _newRectSettings.CaptureInput;
    }

    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        if (_shader is null)
        {
            if (!SetupShader())
            {
                return;
            }
        }
        
        UIRectData data = new UIRectData()
        {
            Position = new AlignedVector3(Bounds.CalculatedPosition),
            Size = new Vector2(Bounds.CalculatedWidth, Bounds.CalculatedHeight),
            TopLeftColor = RectSettings.Color.TopLeft,
            TopRightColor = RectSettings.Color.TopRight,
            BottomLeftColor = RectSettings.Color.BottomLeft,
            BottomRightColor = RectSettings.Color.BottomRight,
            BorderRadius = new Vector4(
                RectSettings.BorderRadius.BottomLeft, 
                RectSettings.BorderRadius.TopLeft, 
                RectSettings.BorderRadius.BottomRight, 
                RectSettings.BorderRadius.TopRight),
            BorderColor = RectSettings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = RectSettings.BorderSize,
        };
        
        if (RectSettings.Color.Visible())
        {
            renderPass.SetUniforms(data);
            renderPass.Bind(_shader);
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }
    }
    
    public Rect Default(ActionRef<RectSettings> defaultSettings)
    {
        _default += defaultSettings;
        _default.Invoke(ref RectSettings);
        _newRectSettings = RectSettings;
        Settings = RectSettings.ToUiSettings();
        return this;
    }

    public Rect OnHover(ActionRef<RectSettings> hover)
    {
        _onHoverEvent += hover;
        return this;
    }

    public Rect OnMouseDown(ActionRef<RectSettings> click)
    {
        _onMouseDownEvent += click;
        return this;
    }
    
    public Rect OnHold(ActionRef<RectSettings> hold)
    {
        _onHoldEvent += hold;
        return this;
    }
    
    public Rect OnMouseUp(ActionRef<RectSettings> click)
    {
        _onMouseUpEvent += click;
        return this;
    }

    public Rect Add(UiElement child)
    {
        Children.Add(child);
        child.Parent = this;
        return this;
    }
}