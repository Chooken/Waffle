using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using VYaml.Emitter;
using VYaml.Parser;

namespace WaffleEngine.Serializer;

public static class Yaml
{
    public static bool TrySerialize(string path, ISerializable obj)
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
        
        obj.Serialize(ref emitter);
        
        File.WriteAllBytes(path, buffer.WrittenSpan);
        return true;
    }

    public static bool TryDeserialize<T>(string path, [NotNullWhen(true)] out T? obj) where T : IDeserializable<T>
    {
        obj = default;
        
        if (!File.Exists(path))
        {
            WLog.Error($"File {path} not found.");
            return false;
        }

        byte[] source = File.ReadAllBytes(path);

        var parser = YamlParser.FromBytes(source);
        
        parser.SkipHeader();

        if (!T.TryDeserialize(ref parser, out obj))
        {
            return false;
        }

        return true;
    }
}