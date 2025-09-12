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
        _commandBufferUsed = false;
    }

    public void Dispose()
    {
        _world.Dispose();
    }
}