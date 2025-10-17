using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using VYaml.Emitter;

namespace WaffleEngine.Serializer;

public static class SceneSerializer
{
    public static bool TrySerialize(string path, Scene scene)
    {
        string? dir = Path.GetDirectoryName(path);

        if (dir is null)
        {
            WLog.Error($"Failed to get the directory of: {path}");
            return false;
        }
        
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        var buffer = new ArrayBufferWriter<byte>();
        var emitter = new Utf8YamlEmitter(buffer);
        var entities = new Arch.Core.Entity[scene._world.CountEntities(new QueryDescription())];
        
        scene._world.GetEntities(new QueryDescription(), entities);
        
        emitter.BeginMapping();
        
        emitter.WriteString("entities");
        
        emitter.BeginSequence();

        foreach (var entity in entities)
        {
            emitter.BeginSequence();
            
            foreach (var component in entity.GetAllComponents())
            {
                emitter.BeginMapping();
                emitter.WriteString("type");
                emitter.WriteString(component.GetType().FullName);
                
                if (component is not ISerializable)
                {
                    if (!Attribute.IsDefined(component.GetType(), typeof(SerializeDefaultAttribute)))
                    {
                        WLog.Warning($"Component {component.GetType().FullName} isn't Serializable will be default");
                    }

                    emitter.EndMapping();
                    continue;
                }
                
                emitter.WriteString("fields");
                
                ISerializable instance = (ISerializable)component;
                instance.Serialize(ref emitter);
                
                emitter.EndMapping();
            }
            
            emitter.EndSequence();
        }
        
        emitter.EndSequence();
        
        emitter.WriteString("queries");
        
        emitter.BeginSequence();

        foreach (var query in scene._update_queries)
        {
            ((ISerializable)query).Serialize(ref emitter);
        }
        
        emitter.EndSequence();
        
        emitter.EndMapping();
        
        File.WriteAllBytes($"{path}.scene", buffer.WrittenSpan);
        
        return true;
    }

    public static bool TryDeserialize(string path, [NotNullWhen(true)] out Scene? scene)
    {
        scene = null;
        
        if (!File.Exists($"{path}.scene"))
        {
            WLog.Error($"Shader file {path}.scene not found.");
            return false;
        }
        
        return true;
    }
}