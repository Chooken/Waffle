using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using VYaml.Emitter;
using VYaml.Parser;

namespace WaffleEngine.Serializer;

public static class Yaml
{
    public static bool TrySerialize(string path, ISerializable obj)
    {
        StartSerialization(out var buffer, out var emitter);
        
        obj.Serialize(ref emitter);
        
        return TryEndSerialization(path, buffer);
    }

    public static void StartSerialization(out ArrayBufferWriter<byte> buffer, out Utf8YamlEmitter emitter)
    {
        buffer = new ArrayBufferWriter<byte>();
        emitter = new Utf8YamlEmitter(buffer);
    }

    public static bool TryEndSerialization(string path, ArrayBufferWriter<byte> buffer)
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
        
        File.WriteAllBytes(path, buffer.WrittenSpan);
        return true;
    }

    public static bool TryDeserialize<T>(string path, [NotNullWhen(true)] out T? obj) where T : IDeserializable<T>
    {
        obj = default;

        if (!TryStartDeserialize(path, out var parser))
        {
            return false;
        }

        return T.TryDeserialize(ref parser, out obj);
    }

    public static bool TryStartDeserialize(string path, out YamlParser parser)
    {
        parser = default;
        
        if (!File.Exists(path))
        {
            WLog.Error($"File {path} not found.");
            return false;
        }

        byte[] source = File.ReadAllBytes(path);

        parser = YamlParser.FromBytes(source);
        
        parser.SkipHeader();
        
        return true;
    }
}