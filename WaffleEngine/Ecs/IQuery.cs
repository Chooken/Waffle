using System.Runtime.CompilerServices;
using Arch.Core;
using VYaml.Emitter;
using VYaml.Parser;

namespace WaffleEngine;

public interface IQuery
{
    public void Run(in World world);
}

public abstract class Query<T> : IQuery, ISerializable, IDeserializable<Query<T>> where T : struct
{
    public void Run(in World world)
    {
        var queryDescription = new QueryDescription().WithAll<T>();
        var query = world.Query(in queryDescription);
        
        foreach(ref var chunk in query.GetChunkIterator())
        {
            var t = chunk.GetFirst<T>();  
            foreach(var entity in chunk)                          
            {
                ref var t1 = ref Unsafe.Add(ref t, entity);
                
                Run(ref t1);
            }
        }
    }
    
    public abstract void Run(ref T component);
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("type");
        emitter.WriteString(this.GetType().FullName);
        
        emitter.WriteString("component count");
        emitter.WriteInt32(1);
        
        emitter.WriteString("component");
        emitter.WriteString(typeof(T).FullName);
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Query<T> query)
    {
        throw new NotImplementedException();
    }
}

public abstract class Query<T1, T2>  : IQuery, ISerializable, IDeserializable<Query<T1, T2>>
{
    public void Run(in World world)
    {
        var queryDescription = new QueryDescription().WithAll<T1, T2>();
        var query = world.Query(in queryDescription);
        
        foreach(ref var chunk in query.GetChunkIterator())
        {
            var t = chunk.GetFirst<T1, T2>();  
            foreach(var entity in chunk)
            {
                ref var t1 = ref Unsafe.Add(ref t.t0, entity);
                ref var t2 = ref Unsafe.Add(ref t.t1, entity);
                
                Run(ref t1, ref t2);
            }
        }
    }
    
    public abstract void Run(ref T1 component, ref T2 component2);
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("type");
        emitter.WriteString(this.GetType().FullName);
        
        emitter.WriteString("component count");
        emitter.WriteInt32(2);
        
        emitter.WriteString("components");
        emitter.BeginSequence();
        emitter.WriteString(typeof(T1).FullName);
        emitter.WriteString(typeof(T2).FullName);
        emitter.EndSequence();
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Query<T1, T2> query)
    {
        throw new NotImplementedException();
    }
}

public abstract class Query<T1, T2, T3>  : IQuery, ISerializable, IDeserializable<Query<T1, T2, T3>>
{
    public void Run(in World world)
    {
        var queryDescription = new QueryDescription().WithAll<T1, T2, T3>();
        var query = world.Query(in queryDescription);
        
        foreach(ref var chunk in query.GetChunkIterator())
        {
            var t = chunk.GetFirst<T1, T2, T3>();  
            foreach(var entity in chunk)                          
            {
                ref var t1 = ref Unsafe.Add(ref t.t0, entity);
                ref var t2 = ref Unsafe.Add(ref t.t1, entity);
                ref var t3 = ref Unsafe.Add(ref t.t2, entity);
                
                Run(ref t1, ref t2, ref t3);
            }
        }
    }
    
    public abstract void Run(ref T1 component, ref T2 component2, ref T3 component3);
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("type");
        emitter.WriteString(this.GetType().FullName);
        
        emitter.WriteString("component count");
        emitter.WriteInt32(3);
        
        emitter.WriteString("components");
        emitter.BeginSequence();
        emitter.WriteString(typeof(T1).FullName);
        emitter.WriteString(typeof(T2).FullName);
        emitter.WriteString(typeof(T3).FullName);
        emitter.EndSequence();
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Query<T1, T2, T3> query)
    {
        throw new NotImplementedException();
    }
}

public abstract class Query<T1, T2, T3, T4>  : IQuery, ISerializable, IDeserializable<Query<T1, T2, T3, T4>>
{
    public void Run(in World world)
    {
        var queryDescription = new QueryDescription().WithAll<T1, T2, T3, T4>();
        var query = world.Query(in queryDescription);
        
        foreach(ref var chunk in query.GetChunkIterator())
        {
            var t = chunk.GetFirst<T1, T2, T3, T4>();  
            foreach(var entity in chunk)                          
            {
                ref var t1 = ref Unsafe.Add(ref t.t0, entity);
                ref var t2 = ref Unsafe.Add(ref t.t1, entity);
                ref var t3 = ref Unsafe.Add(ref t.t2, entity);
                ref var t4 = ref Unsafe.Add(ref t.t3, entity);
                
                Run(ref t1, ref t2, ref t3, ref t4);
            }
        }
    }
    
    public abstract void Run(ref T1 component, ref T2 component2, ref T3 component3, ref T4 component4);
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("type");
        emitter.WriteString(this.GetType().FullName);
        
        emitter.WriteString("component count");
        emitter.WriteInt32(4);
        
        emitter.WriteString("components");
        emitter.BeginSequence();
        emitter.WriteString(typeof(T1).FullName);
        emitter.WriteString(typeof(T2).FullName);
        emitter.WriteString(typeof(T3).FullName);
        emitter.WriteString(typeof(T4).FullName);
        emitter.EndSequence();
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Query<T1, T2, T3, T4> query)
    {
        throw new NotImplementedException();
    }
}

public abstract class Query<T1 ,T2, T3, T4, T5>  : IQuery, ISerializable, IDeserializable<Query<T1 ,T2, T3, T4, T5>>
{
    public void Run(in World world)
    {
        var queryDescription = new QueryDescription().WithAll<T1, T2, T3, T4, T5>();
        var query = world.Query(in queryDescription);
        
        foreach(ref var chunk in query.GetChunkIterator())
        {
            var t = chunk.GetFirst<T1, T2, T3, T4, T5>();  
            foreach(var entity in chunk)                          
            {
                ref var t1 = ref Unsafe.Add(ref t.t0, entity);
                ref var t2 = ref Unsafe.Add(ref t.t1, entity);
                ref var t3 = ref Unsafe.Add(ref t.t2, entity);
                ref var t4 = ref Unsafe.Add(ref t.t3, entity);
                ref var t5 = ref Unsafe.Add(ref t.t4, entity);
                
                Run(ref t1, ref t2, ref t3, ref t4, ref t5);
            }
        }
    }
    
    public abstract void Run(ref T1 component, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5);
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("type");
        emitter.WriteString(this.GetType().FullName);
        
        emitter.WriteString("component count");
        emitter.WriteInt32(5);
        
        emitter.WriteString("components");
        emitter.BeginSequence();
        emitter.WriteString(typeof(T1).FullName);
        emitter.WriteString(typeof(T2).FullName);
        emitter.WriteString(typeof(T3).FullName);
        emitter.WriteString(typeof(T4).FullName);
        emitter.WriteString(typeof(T5).FullName);
        emitter.EndSequence();
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Query<T1 ,T2, T3, T4, T5> query)
    {
        throw new NotImplementedException();
    }
}

public abstract class Query<T1, T2, T3, T4, T5, T6>  : IQuery, ISerializable, IDeserializable<Query<T1, T2, T3, T4, T5, T6>>
{
    public void Run(in World world)
    {
        var queryDescription = new QueryDescription().WithAll<T1, T2, T3, T4, T5, T6>();
        var query = world.Query(in queryDescription);
        
        foreach(ref var chunk in query.GetChunkIterator())
        {
            var t = chunk.GetFirst<T1, T2, T3, T4, T5, T6>();  
            foreach(var entity in chunk)                          
            {
                ref var t1 = ref Unsafe.Add(ref t.t0, entity);
                ref var t2 = ref Unsafe.Add(ref t.t1, entity);
                ref var t3 = ref Unsafe.Add(ref t.t2, entity);
                ref var t4 = ref Unsafe.Add(ref t.t3, entity);
                ref var t5 = ref Unsafe.Add(ref t.t4, entity);
                ref var t6 = ref Unsafe.Add(ref t.t5, entity);
                
                Run(ref t1, ref t2, ref t3, ref t4, ref t5, ref t6);
            }
        }
    }
    
    public abstract void Run(ref T1 component, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6);
    
    public void Serialize(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        
        emitter.WriteString("type");
        emitter.WriteString(this.GetType().FullName);
        
        emitter.WriteString("component count");
        emitter.WriteInt32(6);
        
        emitter.WriteString("components");
        emitter.BeginSequence();
        emitter.WriteString(typeof(T1).FullName);
        emitter.WriteString(typeof(T2).FullName);
        emitter.WriteString(typeof(T3).FullName);
        emitter.WriteString(typeof(T4).FullName);
        emitter.WriteString(typeof(T5).FullName);
        emitter.WriteString(typeof(T6).FullName);
        emitter.EndSequence();
        
        emitter.EndMapping();
    }

    public static bool TryDeserialize(ref YamlParser parser, out Query<T1, T2, T3, T4, T5, T6> query)
    {
        throw new NotImplementedException();
    }
}