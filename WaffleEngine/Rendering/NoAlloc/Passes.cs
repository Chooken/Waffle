using SDL3;

namespace WaffleEngine.Rendering.NoAlloc;

public interface IPass
{
    public void Start(in Queue queue);
    public void End();
}

public struct CopyPass : IPass
{
    internal IntPtr Handle;
    
    public void Start(in Queue queue)
    {
        Handle = SDL.BeginGPUCopyPass(queue.Handle);
    }

    public void End()
    {
        SDL.EndGPUCopyPass(Handle);
    }

    public void UploadToGpu<T>(T value) where T : IGpuUploadable
    {
        value.UploadToGpu(Handle);
    }
}

public struct RenderPass(ColorTargetSettings colorTargetSettings, Pipeline pipeline) : IPass
{
    internal IntPtr Handle;
    
    public unsafe void Start(in Queue queue)
    {
        if (colorTargetSettings.RenderTexture.Texture == IntPtr.Zero)
        {
            WLog.Error("RenderPass was given a null texture");
            return;
        }
        
        SDL.GPUColorTargetInfo colorTargetInfo = new SDL.GPUColorTargetInfo();
        colorTargetInfo.Texture = colorTargetSettings.RenderTexture.Texture;
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
        
        pipeline.Bind(Handle);
    }

    public void End()
    {
        SDL.EndGPURenderPass(Handle);
    }

    public void Bind<T>(T value, uint slot) where T : IGpuBindable
    {
        value.Bind(Handle, slot);
    }

    public void DrawPrimatives(uint numberOfVertices, uint numberOfInstances, uint firstVertex, uint firstInstance)
    {
        SDL.DrawGPUPrimitives(Handle, numberOfVertices, numberOfInstances, firstVertex, firstInstance);
    }

    public void DrawIndexedPrimatives(uint numberOfIndices, uint numberOfInstances, uint firstIndex, short vertexOffset,
        uint firstInstance)
    {
        SDL.DrawGPUIndexedPrimitives(Handle, numberOfIndices, numberOfInstances, firstIndex, vertexOffset, firstInstance);
    }
}

public struct BlitPass(RenderTexture source, RenderTexture destination) : IPass
{
    public void Start(in Queue queue)
    {
        if (queue.Handle == IntPtr.Zero)
        {
            WLog.Error("The Queue has already been submitted");
            return;
        }
        
        var blitInfo = new SDL.GPUBlitInfo
        {
            ClearColor = new SDL.FColor(0, 0, 0, 0),
            Source = new SDL.GPUBlitRegion
            {
                W = source.Width,
                H = source.Height,
                X = 0,
                Y = 0,
                Texture = source.Texture,
            },
            Destination = new SDL.GPUBlitRegion
            {
                W = destination.Width,
                H = destination.Height,
                X = 0,
                Y = 0,
                Texture = destination.Texture,
            },
            Filter = SDL.GPUFilter.Nearest,
            LoadOp = SDL.GPULoadOp.Clear,
            Cycle = 1,
        };

        SDL.BlitGPUTexture(queue.Handle, blitInfo);
    }
    
    public void End() {}
}