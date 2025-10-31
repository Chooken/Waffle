using System.Numerics;
using VYaml.Emitter;
using VYaml.Parser;

namespace WaffleEngine;

public struct Transform : ISerializable, IDeserializable<Transform>
{
    private Vector3 _position;
    private Vector3 _scale;
    private Quaternion _rotation;

    public Transform(Vector3 position, Vector3 scale, Quaternion rotation)
    {
        _position = position;
        _scale = scale;
        _rotation = rotation;
    }
    
    public Vector3 Position
    {
        get => _position;
        set => SetPosition(value);
    }

    public Vector3 Scale
    {
        get => _scale; 
        set => SetScale(value); 
    }

    public Quaternion Rotation
    {
        get => _rotation; 
        set => SetRotation(value); 
    }

    public Matrix4x4 TransformationMatrix
    {
        get
        {
            if (NeedsRebuild || _transformationMat == default)
                BuildMatrix();

            return _transformationMat;
        }
    }

    private Matrix4x4 _transformationMat;
    
    public bool NeedsRebuild;

    public void SetPosition(Vector3 position)
    {
        _position = position;
        NeedsRebuild = true;
    }
    
    public void SetScale(Vector3 scale)
    {
        _scale = scale;
        NeedsRebuild = true;
    }
    
    public void SetRotation(Quaternion rotation)
    {
        _rotation = rotation;
        NeedsRebuild = true;
    }

    public void BuildMatrix()
    {
        if (!NeedsRebuild && _transformationMat != new Matrix4x4())
            return;
        
        Matrix4x4 translation = Matrix4x4.CreateTranslation(_position);
        Matrix4x4 scale = Matrix4x4.CreateScale(_scale);
        Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(_rotation);

        _transformationMat = translation * scale * rotation;
    }
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("position");
        emitter.BeginSequence(SequenceStyle.Flow);
        
        emitter.WriteFloat(Position.x);
        emitter.WriteFloat(Position.y);
        emitter.WriteFloat(Position.z);
        
        emitter.EndSequence();
        
        emitter.WriteString("scale");
        emitter.BeginSequence(SequenceStyle.Flow);
        
        emitter.WriteFloat(Scale.x);
        emitter.WriteFloat(Scale.y);
        emitter.WriteFloat(Scale.z);
        
        emitter.EndSequence();
        
        emitter.WriteString("rotation");
        emitter.BeginSequence(SequenceStyle.Flow);
        
        emitter.WriteFloat(Rotation.X);
        emitter.WriteFloat(Rotation.Y);
        emitter.WriteFloat(Rotation.Z);
        emitter.WriteFloat(Rotation.W);
        
        emitter.EndSequence();
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Transform transform)
    {
        throw new NotImplementedException();
    }
}