using System.Numerics;
using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public struct Camera : ISerializable
{
    private Window _window;
    
    private float _fov;
    private float _near;
    private float _far;

    private Matrix4x4 _projectionMat;
    private bool _needsBuild;

    public float Width => (float)_window.Width / _window.Height * _fov;
    public float Height => _fov;

    public Window Window => _window;
    public float Fov => _fov;
    public float Near => _near;
    public float Far => _far;

    public Camera(Window window, float fov, float near, float far)
    {
        _window = window;
        _fov = fov;
        _near = near;
        _far = far;
        _needsBuild = true;
    }

    public void SetWindow(Window window)
    {
        _window = window;
        _needsBuild = true;
    }
    
    public void SetFov(float fov)
    {
        _fov = fov;
        _needsBuild = true;
    }
    
    public void SetNear(float near)
    {
        _near = near;
        _needsBuild = true;
    }
    
    public void SetFar(float far)
    {
        _far = far;
        _needsBuild = true;
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        if (_needsBuild || _projectionMat == new Matrix4x4())
            BuildProjectionMatrix();

        return _projectionMat;
    }

    private void BuildProjectionMatrix()
    {
        _projectionMat = Matrix4x4.CreateOrthographic(Width, Height, _near, _far);
    }

    public Vector2 ScreenToClipSpace(Vector2 position)
    {
        return new Vector2(
            position.x / _window.Width * 2 - 1, 1 - (position.y / _window.Height * 2));
    }
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("window-handle");
        emitter.WriteString(_window.WindowHandle);
        
        emitter.EndMapping();
    }

    public bool TryDeserialize(ref YamlParser parser)
    {
        throw new NotImplementedException();
    }
}