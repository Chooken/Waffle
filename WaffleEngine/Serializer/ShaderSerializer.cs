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
            WLog.Error($"Failed to get the directory of: {path}");
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
        emitter.WriteString(shader.VertexEntry);
        
        emitter.WriteString("fragment-entry");
        emitter.WriteString(shader.FragmentEntry);
        
        emitter.WriteString("format");
        emitter.WriteString(shader.Format.ToString());
        
        emitter.WriteString("Samplers");
        emitter.WriteInt32(shader.Samplers);
        
        emitter.WriteString("UniformBuffers");
        emitter.WriteInt32(shader.UniformBuffers);
        
        emitter.WriteString("StorageBuffers");
        emitter.WriteInt32(shader.StorageBuffers);
        
        emitter.WriteString("StorageTextures");
        emitter.WriteInt32(shader.StorageTextures);
        
        emitter.EndMapping();
        
        File.WriteAllBytes($"{path}.shaderdata", shader.Bytecode);
        File.WriteAllBytes($"{path}.shaderinfo", buffer.WrittenSpan);

        return true;
    }

    public static bool TryDeserialize(string path, [NotNullWhen(true)] out Shader? shader)
    {
        shader = null;

        if (!File.Exists($"{path}.shaderinfo"))
        {
            WLog.Error($"Shader file {path}.shaderinfo not found.");
            return false;
        }
        
        if (!File.Exists($"{path}.shaderdata"))
        {
            WLog.Error($"Shader file {path}.shaderdata not found.");
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
            WLog.Error($"Failed to parse shader format from {path}.shaderinfo");
            return false;
        }
        
        if (!parser.TryReadScalarAsString(out string? samplersKey))
        {
            WLog.Error($"Expected Samplers found: {samplersKey}");
            return false;
        }
        if (!parser.TryReadScalarAsUInt32(out uint samplers))
        {
            WLog.Error("Failed to read Samplers value from file");
            return false;
        }
        
        if (!parser.TryReadScalarAsString(out string? uniformBuffersKey))
        {
            WLog.Error($"Expected Uniform Buffers found: {uniformBuffersKey}");
            return false;
        }
        if (!parser.TryReadScalarAsUInt32(out uint uniformBuffers))
        {
            WLog.Error("Failed to read Uniform Buffers value from file");
            return false;
        }
        
        if (!parser.TryReadScalarAsString(out string? storageBufferKey))
        {
            WLog.Error($"Expected Storage Buffers found: {storageBufferKey}");
            return false;
        }
        if (!parser.TryReadScalarAsUInt32(out uint storageBuffers))
        {
            WLog.Error("Failed to read Storage Buffers value from file");
            return false;
        }
        
        if (!parser.TryReadScalarAsString(out string? storageTexturesKey))
        {
            WLog.Error($"Expected Storage Textures found: {storageTexturesKey}");
            return false;
        }
        if (!parser.TryReadScalarAsUInt32(out uint storageTextures))
        {
            WLog.Error("Failed to read Storage Textures value from file");
            return false;
        }

        shader = new Shader(
            shaderData, 
            vertexEntry!, 
            fragmentEntry!, 
            format,
            samplers,
            uniformBuffers,
            storageBuffers,
            storageTextures);
        
        return true;
    }
}