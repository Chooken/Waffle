using System.Reflection;
using System.Runtime.CompilerServices;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using VYaml.Emitter;
using VYaml.Parser;

namespace WaffleEngine;

public sealed class Scene : IScene, ISerializable, IDeserializable<Scene>
{
    private EcsWorld _world = new EcsWorld();
    
    internal List<IQuery> _init_queries = new List<IQuery>();
    internal List<IQuery> _update_queries = new List<IQuery>();
    internal List<IQuery> _dispose_queries = new List<IQuery>();

    public Entity InstantiateEntity(Transform transform = new Transform()) => _world.InstantiateEntity(transform);

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

    public bool OnSceneLoaded()
    {
        RunQueries(QueryEvent.Init);
        _world.RunArchCommandBuffer();
        return true;
    }

    public void OnSceneUpdate()
    {
        RunQueries(QueryEvent.Update);
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
            _world.RunQuery(query);
        }
        
        _world.RunArchCommandBuffer();
    }

    public void OnSceneExit()
    {
        RunQueries(QueryEvent.Dispose);
        _world.Dispose();
    }

    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        _world.Serialize(ref emitter);
        
        emitter.WriteString("queries");
        
        emitter.BeginSequence();

        foreach (var query in _update_queries)
        {
            ((ISerializable)query).Serialize(ref emitter);
        }
        
        emitter.EndSequence();
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Scene scene)
    {
        throw new NotImplementedException();
    }
}