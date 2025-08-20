using System.Numerics;
using VYaml.Emitter;
using VYaml.Parser;

namespace WaffleEngine;

public struct Camera : ISerializable
{
    private WindowSdl _window;
    private float _fov;
    private float _near;
    private float _far;

    private Matrix4x4 _viewMat;
    private bool _needsViewBuild;

    public float Width => (float)_window.Width / _window.Height * _fov;
    public float Height => _fov;

    public WindowSdl Window => _window;
    public float Fov => _fov;
    public float Near => _near;
    public float Far => _far;

    public Camera(WindowSdl window, float fov, float near, float far)
    {
        _window = window;
        _fov = fov;
        _near = near;
        _far = far;
        _needsViewBuild = true;
    }

    public void SetWindow(WindowSdl window)
    {
        _window = window;
        _needsViewBuild = true;
    }
    
    public void SetFov(float fov)
    {
        _fov = fov;
        _needsViewBuild = true;
    }
    
    public void SetNear(float near)
    {
        _near = near;
        _needsViewBuild = true;
    }
    
    public void SetFar(float far)
    {
        _far = far;
        _needsViewBuild = true;
    }

    public Matrix4x4 GetViewMatrix()
    {
        if (_needsViewBuild || _viewMat == new Matrix4x4())
            BuildViewMatrix();

        return _viewMat;
    }

    private void BuildViewMatrix()
    {
        _viewMat = Matrix4x4.CreateOrthographic(Width, Height, _near, _far);
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