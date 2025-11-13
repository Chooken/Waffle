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
    
    public UiRenderer(Window window)
    {
        _window = window;
        _window.OnWindowResized += OnWindowResize;
        _windowRoot = new Rect()
            .Default(() => new RectSettings()
            {
                Width = Ui.Fixed(_window.Width / _window.GetDisplayScale()),
                Height = Ui.Fixed(_window.Height / _window.GetDisplayScale()),
            });
        _uiTexture = new GpuTexture(_window);
    }

    public void UpdateUi()
    {
        _windowRoot.PropagateUpdate(_window, true);
    }

    public GpuTexture Render(ImQueue queue)
    {
        if (Root is null)
        {
            return _uiTexture;
        }
        
        Ui.RenderToTexture(_windowRoot, queue, _window.GetDisplayScale(), in _uiTexture);

        return _uiTexture;
    }

    private void OnWindowResize(Vector2 size)
    {
        _uiTexture.Resize((uint)size.x, (uint)size.y);
    }

    public void SetEnforceWindowSize(bool value)
    {
        if (value && _root is not null)
        {
            _root.Layout.CalculateFitSize(_root, true);
            _root.Layout.CalculateFitSize(_root, false);
            _window.SetMinimumSize((int)_root.Bounds.CalculatedWidth, (int)_root.Bounds.CalculatedHeight);
        }
        else
        {
            _window.SetMinimumSize(0, 0);
        }
    }

    public void Dispose()
    {
        _window.OnWindowResized -= OnWindowResize;
    }
}