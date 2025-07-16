using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using VYaml;
using VYaml.Emitter;
using VYaml.Parser;
using WaffleEngine.Rendering;

namespace WaffleEngine.Serializer;

public static class ShaderSerializer
{
    public static bool TrySerialize(string path, Shader shader)
    {
        string? dir = Path.GetDirectoryName(path);

        if (dir is null)
        {
            WLog.Error($"Failed to get the directory of: {path}", "Shader Serializer");
            return false;
        }
        
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        var buffer = new ArrayBufferWriter<byte>();
        var emitter = new Utf8YamlEmitter(buffer);
        
        emitter.BeginMapping();
        
        emitter.WriteString("vertex-entry");
        emitter.WriteString(shader._vertexEntry);
        
        emitter.WriteString("fragment-entry");
        emitter.WriteString(shader._fragmentEntry);
        
        emitter.WriteString("format");
        emitter.WriteString(shader._format.ToString());
        
        emitter.EndMapping();
        
        File.WriteAllBytes($"{path}.shaderdata", shader._bytecode);
        File.WriteAllBytes($"{path}.shaderinfo", buffer.WrittenSpan);

        return true;
    }

    public static bool TryDeserialize(string path, [NotNullWhen(true)] out Shader? shader)
    {
        shader = null;

        if (!File.Exists($"{path}.shaderinfo"))
        {
            WLog.Error($"Shader file {path}.shaderinfo not found.", "Shader Deserializer");
            return false;
        }
        
        if (!File.Exists($"{path}.shaderdata"))
        {
            WLog.Error($"Shader file {path}.shaderdata not found.", "Shader Deserializer");
            return false;
        }

        byte[] shaderInfo = File.ReadAllBytes($"{path}.shaderinfo");
        byte[] shaderData = File.ReadAllBytes($"{path}.shaderdata");

        var parser = YamlParser.FromBytes(shaderInfo);
        
        parser.SkipAfter(ParseEventType.DocumentStart);
        
        parser.ReadWithVerify(ParseEventType.MappingStart);
        
        if (!parser.TryReadScalarAsString(out string? vertexKey))
        {
            WLog.Error($"Expected Vertex Entry found: {vertexKey}");
            return false;
        }
        if (!parser.TryReadScalarAsString(out string? vertexEntry))
        {
            WLog.Error("Failed to read Vertex Entry value from file");
            return false;
        }

        if (!parser.TryReadScalarAsString(out string? fragmentKey))
        {
            WLog.Error($"Expected Vertex Entry found: {fragmentKey}");
            return false;
        }
        if (!parser.TryReadScalarAsString(out string? fragmentEntry))
        {
            WLog.Error("Failed to read Vertex Entry value from file");
            return false;
        }

        if (!parser.TryReadScalarAsString(out string? formatKey))
        {
            WLog.Error($"Expected Vertex Entry found: {formatKey}");
            return false;
        }
        if (!parser.TryReadScalarAsString(out string? formatString))
        {
            WLog.Error("Failed to read Vertex Entry value from file");
            return false;
        }

        if (!Enum.TryParse(formatString, out ShaderFormat format))
        {
            WLog.Error($"Failed to parse shader format from {path}.shaderinfo", "Shader Deserializer");
            return false;
        }

        shader = new Shader(shaderData, vertexEntry!, fragmentEntry!, format);
        
        return true;
    }
}