using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine.Serializer;

namespace WaffleEngine;

public class EcsWorld : ISerializable, IDeserializable<EcsWorld>, IDisposable
{
    private readonly World _world = World.Create();
    
    private bool _commandBufferUsed = false;
    private CommandBuffer _commandBuffer = new CommandBuffer();
    
    public Entity InstantiateEntity(Transform transform = new Transform())
    {
        Entity entity = new Entity
        {
            ParentScene = this,
            ArchEntity = _world.Create(transform),
        };

        return entity;
    }
    
    public void AddComponentToEntity<T>(Entity entity, T component) where T : struct
    {
        _commandBuffer.Add(entity.ArchEntity, component);
        _commandBufferUsed = true;
    }

    public void RunQuery(IQuery query)
    {
        query.Run(in _world);
    }
    
    public void RunArchCommandBuffer()
    {
        if (!_commandBufferUsed)
            return;
        
        _commandBuffer.Playback(_world);
        _commandBufferUsed = false;
    }

    public void Dispose()
    {
        _world.Dispose();
    }

    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        var entities = new Arch.Core.Entity[_world.CountEntities(new QueryDescription())];
        
        _world.GetEntities(new QueryDescription(), entities);
        
        emitter.WriteString("entities");
        
        emitter.BeginSequence();

        foreach (var entity in entities)
        {
            emitter.BeginSequence();
            
            foreach (var component in entity.GetAllComponents())
            {
                if (component is null)
                    continue;
                
                emitter.BeginMapping();
                emitter.WriteString("type");
                emitter.WriteString(component.GetType().FullName!);
                
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
    }

    public static bool TryDeserialize(ref YamlParser parser, out EcsWorld world)
    {
        throw new NotImplementedException();
    }
}