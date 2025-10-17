using System.Runtime.CompilerServices;
using SDL3;

namespace WaffleEngine.Rendering.Immediate;

public struct ImQueue()
{
    private static Material? _blitMaterial;

    public IntPtr Handle { get; private set; } = SDL.AcquireGPUCommandBuffer(Device.Handle);

    public bool Active => Handle != IntPtr.Zero;

    public ImRenderPass AddRenderPass(ColorTargetSettings colorTargetSettings)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("Command Queue wasn't created before calling AddRenderPass");
        }
        
        return new ImRenderPass(this, colorTargetSettings);
    }

    public ImCopyPass AddCopyPass()
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("Command Queue wasn't created before calling AddCopyPass");
        }
        
        return new ImCopyPass(this);
    }

    public void AddBlitPass(GpuTexture source, GpuTexture destination, bool clear)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("Command Queue wasn't created before calling AddCopyPass");
            return;
        }
        
        if (_blitMaterial is null)
            CreateBlitMaterial();

        _blitMaterial.Clear();
        _blitMaterial.AddTexture(source, 0);
    
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0.0f, 0.0f, 0.0f, 0.0f),
            GpuTexture = destination,
            LoadOperation = clear ? LoadOperation.Clear : LoadOperation.Load,
            StoreOperation = StoreOperation.Store,
        };
        
        ImRenderPass renderPass = AddRenderPass(colorTargetSettings);
        renderPass.Bind(_blitMaterial);
        renderPass.DrawPrimatives(3, 1, 0, 0);
        renderPass.End();
    }

    private void CreateBlitMaterial()
    {
        if (!ShaderManager.TryGetShader("BuiltinShaders/blit", out Shader? shader))
        {
            WLog.Error("Shader not found");
            return;
        }
    
        shader.SetPipeline(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
        });
    
        _blitMaterial = new Material(shader);
    }

    public unsafe void SetUniforms<T>(T value)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("Command Queue wasn't created before calling AddCopyPass");
            return;
        }
        
        SDL.PushGPUVertexUniformData(Handle, 0, (IntPtr)Unsafe.AsPointer(ref value), (uint) Unsafe.SizeOf<T>());
        SDL.PushGPUFragmentUniformData(Handle, 0, (IntPtr)Unsafe.AsPointer(ref value), (uint) Unsafe.SizeOf<T>());
    }
    
    public bool TryGetSwapchainTexture(Window window, ref GpuTexture texture)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("Command buffer wasn't acquired");
            return false;
        }
        
        return window.TryGetSwapchainTexture(this, ref texture);
    }

    public void Submit()
    {
        if (!SDL.SubmitGPUCommandBuffer(Handle))
        {
            WLog.Info("Failed submitting GPU CommandBuffer");
        }
        
        Handle = IntPtr.Zero;
    }
}

public struct ImRenderPass
{
    public IntPtr Handle { get; private set; }
    public ImQueue Queue { get; }

    public unsafe ImRenderPass(ImQueue queue, ColorTargetSettings colorTargetSettings)
    {
        if (colorTargetSettings.GpuTexture.Handle == IntPtr.Zero)
        {
            WLog.Error("ImRenderPass was given a null texture");
            return;
        }

        Queue = queue;
        
        SDL.GPUColorTargetInfo colorTargetInfo = new SDL.GPUColorTargetInfo();
        colorTargetInfo.Texture = colorTargetSettings.GpuTexture.Handle;
        colorTargetInfo.ClearColor = new SDL.FColor
        {
            R = colorTargetSettings.ClearColor.r, 
            G = colorTargetSettings.ClearColor.g, 
            B = colorTargetSettings.ClearColor.b, 
            A = colorTargetSettings.ClearColor.a
        };
        colorTargetInfo.LoadOp = (SDL.GPULoadOp) colorTargetSettings.LoadOperation;
        colorTargetInfo.StoreOp = (SDL.GPUStoreOp) colorTargetSettings.StoreOperation;
        
        Handle = SDL.BeginGPURenderPass(queue.Handle, (IntPtr)(&colorTargetInfo), 1, IntPtr.Zero);
    }

    public void DrawPrimatives(
        uint numberOfVertices, 
        uint numberOfInstances,
        uint firstVertex, 
        uint firstInstance)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("The render pass is not active it either finished or hadn't started");
            return;
        }
        
        SDL.DrawGPUPrimitives(Handle, numberOfVertices, numberOfInstances, firstVertex, firstInstance);
    }

    public void DrawIndexedPrimatives(
        uint numberOfIndices,
        uint numberOfInstances,
        uint firstIndex,
        short vertexOffset,
        uint firstInstance)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("The render pass is not active it either finished or hadn't started");
            return;
        }
        
        SDL.DrawGPUIndexedPrimitives(Handle, numberOfIndices, numberOfInstances, firstIndex, vertexOffset, firstInstance);
    }

    public void Bind(IGpuBindable bindable, uint slot = 0)
    {
        bindable.Bind(this, slot);
    }

    public void End()
    {
        if (Handle == IntPtr.Zero)
        {
            return;
        }
        
        SDL.EndGPURenderPass(Handle);
        Handle = IntPtr.Zero;
    }
}

public struct ImCopyPass(ImQueue queue)
{
    public IntPtr Handle { get; private set; } = SDL.BeginGPUCopyPass(queue.Handle);
    public ImQueue Queue { get; } = queue;

    public void Upload(IGpuUploadable uploadable)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("The copy pass is not active it either finished or hadn't started");
            return;
        }
        
        uploadable.UploadToGpu(this);
    }

    public void End()
    {
        if (Handle == IntPtr.Zero)
        {
            return;
        }
        
        SDL.EndGPUCopyPass(Handle);
        Handle = IntPtr.Zero;
    }
}