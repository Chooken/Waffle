using System.Numerics;
using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public struct Camera : ISerializable
{
    private float _fov;
    private float _near;
    private float _far;

    public float Width(float width, float height) => width / height * _fov;
    public float Height => _fov;

    public float Fov => _fov;
    public float Near => _near;
    public float Far => _far;

    public Camera(float fov, float near, float far)
    {
        _fov = fov;
        _near = near;
        _far = far;
    }
    
    public void SetFov(float fov)
    {
        _fov = fov;
    }
    
    public void SetNear(float near)
    {
        _near = near;
    }
    
    public void SetFar(float far)
    {
        _far = far;
    }

    public Matrix4x4 GetProjectionMatrix(uint width, uint height)
    {
        return Matrix4x4.CreateOrthographic(Width(width, height), Height, _near, _far);;
    }
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("fov");
        emitter.WriteFloat(_fov);
        
        emitter.WriteString("near");
        emitter.WriteFloat(_near);
        
        emitter.WriteString("far");
        emitter.WriteFloat(_far);
        
        emitter.EndMapping();
    }

    public bool TryDeserialize(ref YamlParser parser)
    {
        throw new NotImplementedException();
    }
}