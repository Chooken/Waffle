using System.Reflection;
using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;

namespace WaffleEngine;

public sealed class Scene : IDisposable
{
    internal readonly World _world = World.Create();
    
    internal List<IQuery> _queries = new List<IQuery>();
    internal List<Type> _disposableComponents = new List<Type>();

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

    public void AddQuery(IQuery query)
    {
        _queries.Add(query);
    }

    internal void AddComponentToEntity<T>(Entity entity, T component) where T : struct
    {
        if (component is IDisposable)
            _disposableComponents.Add(typeof(T));
        
        if (component is ISceneInit init)
            init.OnInit();
        
        _commandBuffer.Add(entity.ArchEntity, component);
        _commandBufferUsed = true;
    }

    internal void Init()
    {
        RunArchCommandBuffer();
    }

    internal void RunQueries()
    {
        foreach (IQuery query in _queries)
        {
            query.Run(in _world);
        }
        
        RunArchCommandBuffer();
    }

    private void RunArchCommandBuffer()
    {
        if (!_commandBufferUsed)
            return;
        
        _commandBuffer.Playback(_world);
        _commandBuffer = new CommandBuffer();
        _commandBufferUsed = false;
    }

    public void Dispose()
    {
        foreach (var type in _disposableComponents)
        {
            var signature = new Signature(type);
            
            var queryDescription = new QueryDescription(
                all: signature
            );
            var query = _world.Query(in queryDescription);
        
            foreach(ref var chunk in query.GetChunkIterator())
            {
                var t = (IDisposable)chunk.GetArray(type).GetValue(0)!;
                
                foreach(var entity in chunk)                          
                {
                    ref var t1 = ref Unsafe.Add(ref t, entity);
                
                    t1.Dispose();
                }
            }
        }
        
        _world.Dispose();
    }
}