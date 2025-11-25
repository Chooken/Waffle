using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class UiRenderer
{
    public UiElement? Root
    {
        get => _root;
        set
        {
            _root = value;
            
            if (_root is null)
            {
                _windowRoot.Children.Clear();
                return;
            }

            if (_windowRoot.Children.Count == 0)
            {
                _windowRoot.Children.Add(_root);
            }
            else _windowRoot.Children[0] = _root;
        }
    }
    private UiElement? _root;

    private UiElement _windowRoot;
    private GpuTexture _uiTexture;
    private Window _window;

    private float _scale;
    
    public UiRenderer(Window window, float scale = 1)
    {
        _window = window;
        _window.OnWindowResized += OnWindowResize;
        _windowRoot = new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Pixels(_window.Width / _scale),
                Height = Ui.Pixels(_window.Height / _scale),
            });
        _uiTexture = new GpuTexture(_window);
        _scale = scale;
    }

    public void UpdateUi()
    {
        _windowRoot.PropagateScale(_scale);
        _windowRoot.PropagateUpdate(_window, true);
    }

    public GpuTexture Render(ImQueue queue, Color clearColor)
    {
        if (Root is null)
        {
            return _uiTexture;
        }
        
        Ui.RenderToTexture(_windowRoot, queue, clearColor, in _uiTexture);

        return _uiTexture;
    }

    private void OnWindowResize(Vector2 size)
    {
        _uiTexture.Resize((uint)_window.PixelWidth, (uint)_window.PixelHeight);
    }

    public void SetEnforceWindowSize(bool value)
    {
        if (value && _root is not null)
        {
            _root.PropagateScale(_scale);
            _root.PropagateUpdate(_window, false);
            _root.Layout.CalculateFitSize(_root, true);
            _root.Layout.CalculateFitSize(_root, false);
            _root.PropagateScale(_scale);
            _window.SetMinimumSize((int)(_root.Bounds.CalculatedWidth), (int)(_root.Bounds.CalculatedHeight));
        }
        else
        {
            _window.SetMinimumSize(0, 0);
        }
    }

    public void SetScale(float scale)
    {
        _scale = scale;
    }

    public void Dispose()
    {
        _window.OnWindowResized -= OnWindowResize;
    }
}