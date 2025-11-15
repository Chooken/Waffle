using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SDL3;
using VYaml.Parser;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public sealed unsafe class Pipeline : IDisposable
{
    internal IntPtr Handle;

    public bool TryBuild(PipelineSettings pipelineSettings, Shader shader)
    {
        if (Handle != IntPtr.Zero)
        {
            Dispose();
        }
        
        SDL.GPUColorTargetDescription colorTargetDescription = new SDL.GPUColorTargetDescription();
        colorTargetDescription.Format = (SDL.GPUTextureFormat)pipelineSettings.ColorTargetFormat;
        colorTargetDescription.BlendState = new SDL.GPUColorTargetBlendState
        {
            EnableBlend = 1,
            ColorBlendOp = (SDL.GPUBlendOp) pipelineSettings.ColorBlendOp,
            AlphaBlendOp = (SDL.GPUBlendOp) pipelineSettings.AlphaBlendOp,
            SrcColorBlendfactor = (SDL.GPUBlendFactor) pipelineSettings.SrcColorBlendFactor,
            DstColorBlendfactor = (SDL.GPUBlendFactor) pipelineSettings.DstColorBlendFactor,
            SrcAlphaBlendfactor = (SDL.GPUBlendFactor) pipelineSettings.SrcAlphaBlendFactor,
            DstAlphaBlendfactor = (SDL.GPUBlendFactor) pipelineSettings.DstAlphaBlendFactor,
        };
        
        SDL.GPUGraphicsPipelineCreateInfo pipelineInfo = new SDL.GPUGraphicsPipelineCreateInfo();
        
        pipelineInfo.TargetInfo = new SDL.GPUGraphicsPipelineTargetInfo
        {
            NumColorTargets = 1,
            ColorTargetDescriptions = (IntPtr)(&colorTargetDescription),
        };
        
        pipelineInfo.PrimitiveType = (SDL.GPUPrimitiveType) pipelineSettings.PrimitiveType;
        
        pipelineInfo.VertexShader = shader.VertexHandle;
        pipelineInfo.FragmentShader = shader.FragmentHandle;
        pipelineInfo.RasterizerState.FillMode = (SDL.GPUFillMode) pipelineSettings.FillMode;
        
        if (pipelineSettings.VertexAttributes is null || pipelineSettings.VertexAttributes.Count == 0)
        {
            Handle = SDL.CreateGPUGraphicsPipeline(Device.Handle, pipelineInfo);
            
            if (Handle == IntPtr.Zero)
                return false;
            
            return true;
        }

        uint totalSize = 0;
        
        SDL.GPUVertexAttribute[] vertexAttributes = new SDL.GPUVertexAttribute[pipelineSettings.VertexAttributes.Count];

        for (uint i = 0; i < pipelineSettings.VertexAttributes.Count; i++)
        {
            vertexAttributes[i].BufferSlot = 0;
            vertexAttributes[i].Location = i;
            vertexAttributes[i].Format = (SDL.GPUVertexElementFormat) pipelineSettings.VertexAttributes[(int)i];
            vertexAttributes[i].Offset = totalSize;

            totalSize += pipelineSettings.VertexAttributes[(int)i].Size();
        }
        
        pipelineInfo.VertexInputState.NumVertexAttributes = (uint)pipelineSettings.VertexAttributes.Count;

        fixed (SDL.GPUVertexAttribute* vertexAttributesPtr = vertexAttributes)
        {
            pipelineInfo.VertexInputState.VertexAttributes = (IntPtr)vertexAttributesPtr;
        }

        SDL.GPUVertexBufferDescription vertexBufferDescription = new SDL.GPUVertexBufferDescription();
        vertexBufferDescription.Slot = 0;
        vertexBufferDescription.InputRate = (SDL.GPUVertexInputRate) pipelineSettings.VertexInputRate;
        vertexBufferDescription.InstanceStepRate = 0;
        vertexBufferDescription.Pitch = totalSize;

        pipelineInfo.VertexInputState.NumVertexBuffers = 1;
        pipelineInfo.VertexInputState.VertexBufferDescriptions = (IntPtr)(&vertexBufferDescription);
        
        Handle = SDL.CreateGPUGraphicsPipeline(Device.Handle, pipelineInfo);
        
        if (Handle == IntPtr.Zero)
            return false;

        return true;
    }
    
    public void Bind(ImRenderPass renderPass)
    {
        SDL.BindGPUGraphicsPipeline(renderPass.Handle, Handle);
    }

    public void Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            SDL.ReleaseGPUGraphicsPipeline(Device.Handle, Handle);
        }
    }
}

public struct PipelineSettings : IDeserializable<PipelineSettings>
{
    public BlendOp ColorBlendOp;
    public BlendOp AlphaBlendOp;
    public BlendFactor SrcColorBlendFactor;
    public BlendFactor SrcAlphaBlendFactor;
    public BlendFactor DstColorBlendFactor;
    public BlendFactor DstAlphaBlendFactor;
    public List<VertexAttributeType>? VertexAttributes;
    public TextureFormat ColorTargetFormat;
    public PrimitiveType PrimitiveType;
    public FillMode FillMode;
    public VertexInputRate VertexInputRate;

    public static PipelineSettings Default => new PipelineSettings
    {
        ColorBlendOp = BlendOp.Add,
        AlphaBlendOp = BlendOp.Add,
        SrcColorBlendFactor = BlendFactor.SrcAlpha,
        SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
        DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
        DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
        ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
        PrimitiveType = PrimitiveType.TriangleList,
        FillMode = FillMode.Fill,
        VertexInputRate = VertexInputRate.Vertex,
    };
    
    public static bool TryDeserialize(ref YamlParser parser, out PipelineSettings pipelineSettings)
    {
        pipelineSettings = PipelineSettings.Default;

        // Read MappingStart.
        if (parser.CurrentEventType != ParseEventType.MappingStart)
        {
            WLog.Error("Pipeline Settings deserializer didn't start with a mapping.");
            return false;
        }

        parser.Read();
        
        while (parser.CurrentEventType != ParseEventType.MappingEnd)
        {
            if (!parser.TryReadScalarAsString(out string? key))
            {
                return false;
            }

            if (key == "VertexAttributes")
            {
                if (parser.CurrentEventType != ParseEventType.SequenceStart)
                    return false;
                
                if (!parser.Read())
                    return false;

                while (parser.CurrentEventType != ParseEventType.SequenceEnd)
                {
                    if (parser.CurrentEventType != ParseEventType.Scalar)
                    {
                        parser.SkipCurrentNode();
                        continue;
                    }

                    if (!parser.TryReadScalarAsString(out var vertexAttributeString))
                        return false;
                    
                    if (!Enum.TryParse(vertexAttributeString, out VertexAttributeType vertexAttributeType))
                        return false;

                    if (pipelineSettings.VertexAttributes is null)
                        pipelineSettings.VertexAttributes = new List<VertexAttributeType>();
                    
                    pipelineSettings.VertexAttributes.Add(vertexAttributeType);
                }
                
                // Read the sequence end.
                parser.Read();
                continue;
            }
            
            if (!parser.TryReadScalarAsString(out string? value))
            {
                return false;
            }

            switch (key)
            {
                case "ColorBlendOp":
                    if (!Enum.TryParse(value, out BlendOp blendOp))
                        return false;
                    pipelineSettings.ColorBlendOp = blendOp;
                    break;
                case "AlphaBlendOp":
                    if (!Enum.TryParse(value, out blendOp))
                        return false;
                    pipelineSettings.AlphaBlendOp = blendOp;
                    break;
                case "SrcColorBlendFactor":
                    if (!Enum.TryParse(value, out BlendFactor blendFactor))
                        return false;
                    pipelineSettings.SrcColorBlendFactor = blendFactor;
                    break;
                case "SrcAlphaBlendFactor":
                    if (!Enum.TryParse(value, out blendFactor))
                        return false;
                    pipelineSettings.SrcAlphaBlendFactor = blendFactor;
                    break;
                case "DstColorBlendFactor":
                    if (!Enum.TryParse(value, out blendFactor))
                        return false;
                    pipelineSettings.DstColorBlendFactor = blendFactor;
                    break;
                case "DstAlphaBlendFactor":
                    if (!Enum.TryParse(value, out blendFactor))
                        return false;
                    pipelineSettings.DstAlphaBlendFactor = blendFactor;
                    break;
                case "ColorTargetFormat":
                    if (!Enum.TryParse(value, out TextureFormat textureFormat))
                        return false;
                    pipelineSettings.ColorTargetFormat = textureFormat;
                    break;
                case "PrimitiveType":
                    if (!Enum.TryParse(value, out PrimitiveType primitiveType))
                        return false;
                    pipelineSettings.PrimitiveType = primitiveType;
                    break;
                case "FillMode":
                    if (!Enum.TryParse(value, out FillMode fillMode))
                        return false;
                    pipelineSettings.FillMode = fillMode;
                    break;
                case "VertexInputRate":
                    if (!Enum.TryParse(value, out VertexInputRate vertexInputRate))
                        return false;
                    pipelineSettings.VertexInputRate = vertexInputRate;
                    break;
                default:
                    WLog.Error($"Unknown field: {key} while parsing pipeline.");
                    return false;
            }
        }

        // Read the mapping end.
        parser.Read();
        
        return true;
    }
}

public enum VertexInputRate
{
    Vertex,
    Instance,
}

public enum FillMode
{
    /// <summary>Polygons will be rendered via rasterization.</summary>
    Fill,
    /// <summary>Polygon edges will be drawn as line segments.</summary>
    Line,
}

public enum PrimitiveType
{
    /// <summary>A series of separate triangles.</summary>
    TriangleList,
    /// <summary>A series of connected triangles.</summary>
    TriangleStrip,
    /// <summary>A series of separate lines.</summary>
    LineList,
    /// <summary>A series of connected lines.</summary>
    LineStrip,
    /// <summary>A series of separate points.</summary>
    PointList,
}

public static class VertexAttribute
{
    public static uint Size(this VertexAttributeType attributeType)
    {
        return attributeType switch
        {
            VertexAttributeType.Byte2
                or VertexAttributeType.Byte2Norm
                or VertexAttributeType.Ubyte2
                or VertexAttributeType.Ubyte2Norm => 2,
            VertexAttributeType.Byte4 
                or VertexAttributeType.Byte4Norm
                or VertexAttributeType.Ubyte4
                or VertexAttributeType.Ubyte4Norm
                or VertexAttributeType.Short2 
                or VertexAttributeType.Short2Norm
                or VertexAttributeType.Ushort2
                or VertexAttributeType.Ushort2Norm
                or VertexAttributeType.Int 
                or VertexAttributeType.Uint
                or VertexAttributeType.Float 
                or VertexAttributeType.Half2 => 4,
            VertexAttributeType.Int2 
                or VertexAttributeType.Float2 
                or VertexAttributeType.Uint2 
                or VertexAttributeType.Short4 
                or VertexAttributeType.Ushort4 
                or VertexAttributeType.Short4Norm 
                or VertexAttributeType.Ushort4Norm 
                or VertexAttributeType.Half4 => 8, 
            VertexAttributeType.Int3 
                or VertexAttributeType.Float3 
                or VertexAttributeType.Uint3 => 12,
            VertexAttributeType.Int4 
                or VertexAttributeType.Float4 
                or VertexAttributeType.Uint4 => 16,
            
            _ => 0
        };
    }
}

public enum VertexAttributeType
{
    Invalid,
    Int,
    Int2,
    Int3,
    Int4,
    Uint,
    Uint2,
    Uint3,
    Uint4,
    Float,
    Float2,
    Float3,
    Float4,
    Byte2,
    Byte4,
    Ubyte2,
    Ubyte4,
    Byte2Norm,
    Byte4Norm,
    Ubyte2Norm,
    Ubyte4Norm,
    Short2,
    Short4,
    Ushort2,
    Ushort4,
    Short2Norm,
    Short4Norm,
    Ushort2Norm,
    Ushort4Norm,
    Half2,
    Half4,
}

public enum BlendOp
{
    Invalid,
    /// <summary>
    /// (source * source_factor) + (destination * destination_factor)
    /// </summary>
    Add,
    /// <summary>
    /// (source * source_factor) - (destination * destination_factor)
    /// </summary>
    Subtract,
    /// <summary>
    /// (destination * destination_factor) - (source * source_factor)
    /// </summary>
    ReverseSubtract,
    /// <summary>min(source, destination)</summary>
    Min,
    /// <summary>max(source, destination)</summary>
    Max,
}

public enum BlendFactor
{
    Invalid,
    /// <summary>0</summary>
    Zero,
    /// <summary>1</summary>
    One,
    /// <summary>source color</summary>
    SrcColor,
    /// <summary>1 - source color</summary>
    OneMinusSrcColor,
    /// <summary>destination color</summary>
    DstColor,
    /// <summary>1 - destination color</summary>
    OneMinusDstColor,
    /// <summary>source alpha</summary>
    SrcAlpha,
    /// <summary>1 - source alpha</summary>
    OneMinusSrcAlpha,
    /// <summary>destination alpha</summary>
    DstAlpha,
    /// <summary>1 - destination alpha</summary>
    OneMinusDstAlpha,
    /// <summary>blend constant</summary>
    ConstantColor,
    /// <summary>1 - blend constant</summary>
    OneMinusConstantColor,
    /// <summary>min(source alpha, 1 - destination alpha)</summary>
    SrcAlphaSaturate,
}