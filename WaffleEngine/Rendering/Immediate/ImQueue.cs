using System.Runtime.CompilerServices;
using SDL3;

namespace WaffleEngine.Rendering.Immediate;

public struct ImQueue()
{
    private static Material? _blitMaterial;
    
    private IntPtr _commandBuffer = SDL.AcquireGPUCommandBuffer(Device.Handle);

    public bool Active => _commandBuffer != IntPtr.Zero;

    public ImRenderPass AddRenderPass(ColorTargetSettings colorTargetSettings)
    {
        if (_commandBuffer == IntPtr.Zero)
        {
            WLog.Error("Command Queue wasn't created before calling AddRenderPass");
        }
        
        return new ImRenderPass(_commandBuffer, colorTargetSettings);
    }

    public ImCopyPass AddCopyPass()
    {
        if (_commandBuffer == IntPtr.Zero)
        {
            WLog.Error("Command Queue wasn't created before calling AddCopyPass");
        }
        
        return new ImCopyPass(_commandBuffer);
    }

    public void AddBlitPass(GpuTexture source, GpuTexture destination, bool clear)
    {
        if (_commandBuffer == IntPtr.Zero)
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
        if (_commandBuffer == IntPtr.Zero)
        {
            WLog.Error("Command Queue wasn't created before calling AddCopyPass");
            return;
        }
        
        SDL.PushGPUVertexUniformData(_commandBuffer, 0, (IntPtr)Unsafe.AsPointer(ref value), (uint) Unsafe.SizeOf<T>());
        SDL.PushGPUFragmentUniformData(_commandBuffer, 0, (IntPtr)Unsafe.AsPointer(ref value), (uint) Unsafe.SizeOf<T>());
    }
    
    public bool TryGetSwapchainTexture(Window window, ref GpuTexture texture)
    {
        if (_commandBuffer == IntPtr.Zero)
        {
            WLog.Error("Command buffer wasn't acquired");
            return false;
        }
        
        IntPtr handle;

        if (!SDL.WaitAndAcquireGPUSwapchainTexture(_commandBuffer, ((WindowSdl)window).WindowPtr, out handle, out uint width, out uint height))
        {
            WLog.Error("Failed to acquire a swapchain texture");
            return false;
        }

        texture.Set(width, height, handle);
        
        return true;
    }

    public void Submit()
    {
        if (!SDL.SubmitGPUCommandBuffer(_commandBuffer))
        {
            WLog.Info("Failed submitting GPU CommandBuffer");
        }
        
        _commandBuffer = IntPtr.Zero;
    }
}

public struct ImRenderPass
{
    public IntPtr Handle => _renderPass;
    
    private IntPtr _renderPass;

    public unsafe ImRenderPass(IntPtr commandBuffer, ColorTargetSettings colorTargetSettings)
    {
        if (colorTargetSettings.GpuTexture.Handle == IntPtr.Zero)
        {
            WLog.Error("ImRenderPass was given a null texture");
            return;
        }
        
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
        
        _renderPass = SDL.BeginGPURenderPass(commandBuffer, (IntPtr)(&colorTargetInfo), 1, IntPtr.Zero);
    }

    public void DrawPrimatives(
        uint numberOfVertices, 
        uint numberOfInstances,
        uint firstVertex, 
        uint firstInstance)
    {
        if (_renderPass == IntPtr.Zero)
        {
            WLog.Error("The render pass is not active it either finished or hadn't started");
            return;
        }
        
        SDL.DrawGPUPrimitives(_renderPass, numberOfVertices, numberOfInstances, firstVertex, firstInstance);
    }

    public void DrawIndexedPrimatives(
        uint numberOfIndices,
        uint numberOfInstances,
        uint firstIndex,
        short vertexOffset,
        uint firstInstance)
    {
        if (_renderPass == IntPtr.Zero)
        {
            WLog.Error("The render pass is not active it either finished or hadn't started");
            return;
        }
        
        SDL.DrawGPUIndexedPrimitives(_renderPass, numberOfIndices, numberOfInstances, firstIndex, vertexOffset, firstInstance);
    }

    public void Bind(IGpuBindable bindable, uint slot = 0)
    {
        bindable.Bind(this, slot);
    }

    public void End()
    {
        if (_renderPass == IntPtr.Zero)
        {
            return;
        }
        
        SDL.EndGPURenderPass(_renderPass);
        _renderPass = IntPtr.Zero;
    }
}

public struct ImCopyPass(IntPtr commandBuffer)
{
    public IntPtr Handle => _copyPass;
    
    private IntPtr _copyPass = SDL.BeginGPUCopyPass(commandBuffer);

    public void Upload(IGpuUploadable uploadable)
    {
        if (_copyPass == IntPtr.Zero)
        {
            WLog.Error("The copy pass is not active it either finished or hadn't started");
            return;
        }
        
        uploadable.UploadToGpu(this);
    }

    public void End()
    {
        if (_copyPass == IntPtr.Zero)
        {
            return;
        }
        
        SDL.EndGPUCopyPass(_copyPass);
        _copyPass = IntPtr.Zero;
    }
}