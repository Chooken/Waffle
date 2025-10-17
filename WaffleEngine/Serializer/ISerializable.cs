using VYaml.Emitter;
using VYaml.Parser;

namespace WaffleEngine;

public interface ISerializable
{
    public void Serialize(ref Utf8YamlEmitter emitter);
}

public interface IDeserializable<T>
{
    public static abstract bool TryDeserialize(ref YamlParser parser, out T obj);
}