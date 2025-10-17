using System.Reflection;
using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;

namespace WaffleEngine;

public sealed class Scene : IDisposable
{
    internal readonly World _world = World.Create();
    
    internal List<IQuery> _init_queries = new List<IQuery>();
    internal List<IQuery> _update_queries = new List<IQuery>();
    internal List<IQuery> _dispose_queries = new List<IQuery>();

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

    public void AddQuery(IQuery query, QueryEvent qEvent = QueryEvent.Update)
    {
        switch (qEvent)
        {
            case QueryEvent.Init:
                _init_queries.Add(query);
                break;
            case QueryEvent.Update:
                _update_queries.Add(query);
                break;
            case QueryEvent.Dispose:
                _dispose_queries.Add(query);
                break;
        }
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

    internal void RunQueries(QueryEvent qEvent)
    {
        List<IQuery> queries;
        
        switch (qEvent)
        {
            case QueryEvent.Init:
                queries = _init_queries;
                break;
            
            case QueryEvent.Update:
                queries = _update_queries;
                break;
            
            case QueryEvent.Dispose:
                queries = _dispose_queries;
                break;
            
            default:
                queries = _update_queries;
                break;
        }
        
        foreach (IQuery query in queries)
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