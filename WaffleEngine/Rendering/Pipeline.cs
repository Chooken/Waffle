using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public sealed unsafe class Pipeline : IDisposable
{
    internal IntPtr Handle;

    public static bool TryBuild(PipelineSettings pipelineSettings, Shader shader, [NotNullWhen(true)] out Pipeline? pipeline)
    {
        pipeline = null;
        
        SDL.GPUColorTargetDescription colorTargetDescription = new SDL.GPUColorTargetDescription();
        colorTargetDescription.Format = (SDL.GPUTextureFormat)pipelineSettings.ColorTargetFormat;
        colorTargetDescription.BlendState = new SDL.GPUColorTargetBlendState
        {
            EnableBlend = true,
            ColorBlendOp = (SDL.GPUBlendOp) pipelineSettings.ColorBlendOp,
            AlphaBlendOp = (SDL.GPUBlendOp) pipelineSettings.AlphaBlendOp,
            SrcColorBlendFactor = (SDL.GPUBlendFactor) pipelineSettings.SrcColorBlendFactor,
            DstColorBlendFactor = (SDL.GPUBlendFactor) pipelineSettings.DstColorBlendFactor,
            SrcAlphaBlendFactor = (SDL.GPUBlendFactor) pipelineSettings.SrcAlphaBlendFactor,
            DstAlphaBlendFactor = (SDL.GPUBlendFactor) pipelineSettings.DstAlphaBlendFactor,
            ColorWriteMask = SDL.GPUColorComponentFlags.R | SDL.GPUColorComponentFlags.G | SDL.GPUColorComponentFlags.B | SDL.GPUColorComponentFlags.A,
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

        pipeline = new();

        if (shader.Inputs.Length == 0)
        {
            pipeline.Handle = SDL.CreateGPUGraphicsPipeline(Device.Handle, pipelineInfo);

            if (pipeline.Handle == IntPtr.Zero)
            {
                WLog.Error(SDL.GetError());
                return false;
            }
            
            return true;
        }

        uint totalSize = 0;
        
        SDL.GPUVertexAttribute[] vertexAttributes = new SDL.GPUVertexAttribute[shader.Inputs.Length];

        for (uint i = 0; i < shader.Inputs.Length; i++)
        {
            vertexAttributes[i].BufferSlot = 0;
            vertexAttributes[i].Location = shader.Inputs[i].Location;
            vertexAttributes[i].Format = (SDL.GPUVertexElementFormat) shader.Inputs[i].AttributeType;
            vertexAttributes[i].Offset = totalSize;

            totalSize += shader.Inputs[i].AttributeType.Size();
        }
        
        pipelineInfo.VertexInputState.NumVertexAttributes = (uint)shader.Inputs.Length;

        SDL.GPUVertexBufferDescription vertexBufferDescription = new SDL.GPUVertexBufferDescription();
        vertexBufferDescription.Slot = 0;
        vertexBufferDescription.InputRate = (SDL.GPUVertexInputRate) pipelineSettings.VertexInputRate;
        vertexBufferDescription.InstanceStepRate = 0;
        vertexBufferDescription.Pitch = totalSize;

        pipelineInfo.VertexInputState.NumVertexBuffers = 1;
        
        fixed (SDL.GPUVertexAttribute* vertexAttributesPtr = vertexAttributes)
        {
            pipelineInfo.VertexInputState.VertexAttributes = (IntPtr)vertexAttributesPtr;
            pipelineInfo.VertexInputState.VertexBufferDescriptions = (IntPtr)(&vertexBufferDescription);

            pipeline.Handle = SDL.CreateGPUGraphicsPipeline(Device.Handle, in pipelineInfo);
        }
        
        if (pipeline.Handle == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }

        return true;
    }
    
    public void Bind(ImRenderPass renderPass)
    {
        SDL.BindGPUGraphicsPipeline(renderPass.Handle, Handle);
    }

    public void Dispose()
    {
        if (Handle == IntPtr.Zero)
        {
            return;
        }
        
        SDL.ReleaseGPUGraphicsPipeline(Device.Handle, Handle);
        Handle = IntPtr.Zero;
    }
    
}

public struct PipelineSettings()
{
    public BlendOp ColorBlendOp = BlendOp.Add;
    public BlendOp AlphaBlendOp = BlendOp.Add;
    public BlendFactor SrcColorBlendFactor = BlendFactor.SrcAlpha;
    public BlendFactor SrcAlphaBlendFactor = BlendFactor.SrcAlpha;
    public BlendFactor DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
    public BlendFactor DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
    public List<VertexAttributeType>? VertexAttributes = new List<VertexAttributeType>()
    {
        VertexAttributeType.Float3,
        VertexAttributeType.Float2,
    };
    public TextureFormat ColorTargetFormat = TextureFormat.B8G8R8A8Unorm;
    public PrimitiveType PrimitiveType = PrimitiveType.TriangleList;
    public FillMode FillMode = FillMode.Fill;
    public VertexInputRate VertexInputRate = VertexInputRate.Vertex;

    public static PipelineSettings Default => new PipelineSettings
    {
        ColorBlendOp = BlendOp.Add,
        AlphaBlendOp = BlendOp.Add,
        SrcColorBlendFactor = BlendFactor.SrcAlpha,
        SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
        DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
        DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
        VertexAttributes = new List<VertexAttributeType>()
        {
            VertexAttributeType.Float3,
            VertexAttributeType.Float2,
        },
        ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
        PrimitiveType = PrimitiveType.TriangleList,
        FillMode = FillMode.Fill,
        VertexInputRate = VertexInputRate.Vertex,
    };
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

public struct VertexInput
{
    public uint Location;
    public VertexAttributeType AttributeType;
}

public static class VertexAttribute
{
    public static uint Size(this VertexAttributeType attributeType)
    {
        return attributeType switch
        {
            VertexAttributeType.Byte2
                or VertexAttributeType.Byte2Norm
                or VertexAttributeType.UByte2
                or VertexAttributeType.UByte2Norm => 2,
            VertexAttributeType.Byte4 
                or VertexAttributeType.Byte4Norm
                or VertexAttributeType.UByte4
                or VertexAttributeType.UByte4Norm
                or VertexAttributeType.Short2 
                or VertexAttributeType.Short2Norm
                or VertexAttributeType.UShort2
                or VertexAttributeType.UShort2Norm
                or VertexAttributeType.Int 
                or VertexAttributeType.UInt
                or VertexAttributeType.Float 
                or VertexAttributeType.Half2 => 4,
            VertexAttributeType.Int2 
                or VertexAttributeType.Float2 
                or VertexAttributeType.UInt2 
                or VertexAttributeType.Short4 
                or VertexAttributeType.UShort4 
                or VertexAttributeType.Short4Norm 
                or VertexAttributeType.UShort4Norm 
                or VertexAttributeType.Half4 => 8, 
            VertexAttributeType.Int3 
                or VertexAttributeType.Float3 
                or VertexAttributeType.UInt3 => 12,
            VertexAttributeType.Int4 
                or VertexAttributeType.Float4 
                or VertexAttributeType.UInt4 => 16,
            
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
    UInt,
    UInt2,
    UInt3,
    UInt4,
    Float,
    Float2,
    Float3,
    Float4,
    Byte2,
    Byte4,
    UByte2,
    UByte4,
    Byte2Norm,
    Byte4Norm,
    UByte2Norm,
    UByte4Norm,
    Short2,
    Short4,
    UShort2,
    UShort4,
    Short2Norm,
    Short4Norm,
    UShort2Norm,
    UShort4Norm,
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