using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.SDLExtra.TTF;

namespace WaffleEngine.Text;

public class AtlasedText
{
    public IntPtr Handle;
    
    private bool _empty;
    private int _fontSize;
    
    private static IntPtr _textEngine;
    private static Shader _shader;

    private Buffer<Vertex> _vertexBuffer = new Buffer<Vertex>(BufferUsage.Vertex);
    private GpuBuffer<int> _indexBuffer = new GpuBuffer<int>(BufferUsage.Index);

    private IntPtr _texture;
    private IntPtr _sampler;

    public AtlasedText(string text, Font font, Color color)
    {
        if (_textEngine == IntPtr.Zero)
        {
            _textEngine = TTF.CreateGPUTextEngine(Device.Handle);
        }

        _fontSize = (int)font.Size;
        
        Handle = TTF.CreateText(_textEngine, font.Handle, text, (uint)text.Length);
        TTF.SetTextColor(Handle, color.r255, color.g255, color.b255, color.a255);
        
        var samplerCreateInfo = new SDL.GPUSamplerCreateInfo
        {
            MinFilter = SDL.GPUFilter.Nearest,
            MagFilter = SDL.GPUFilter.Nearest,
            MipmapMode = SDL.GPUSamplerMipmapMode.Nearest,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeW = SDL.GPUSamplerAddressMode.Repeat,
        };
        
        _sampler = SDL.CreateGPUSampler(Device.Handle, samplerCreateInfo);
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/textured-quad", out _shader))
        {
            Log.Error("Shader not found");
            return;
        }
    }

    public void SetColor(Color color)
    {
        TTF.SetTextColorFloat(Handle, color.r, color.g, color.b, color.a);
    }

    public void SetText(string text)
    {
        TTF.SetTextString(Handle, text, (uint)text.Length);
    }

    public void SetWrapWidth(int width)
    {
        TTF.SetTextWrapWidth(Handle, Math.Max(width, _fontSize));
    }

    public unsafe void Update()
    {
        var sequence = (GPUAtlasDrawSequence*)TTF.GetGPUTextDrawData(Handle);

        if (sequence is null)
        {
            WLog.Error($"{SDL.GetError()}");
            _empty = true;
            return;
        }

        _empty = false;
        
        var formatted = sequence->AsFormatted();
        
        _vertexBuffer.Clear();
        
        for (int i = 0; i < formatted.Vertices.Length; i++)
        {
            _vertexBuffer.Add(new Vertex()
            {
                Position = formatted.Vertices[i],
                Uv = formatted.UVs[i]
            });
        }

        _texture = formatted.AtlasTexture;

        ImQueue queue = new ImQueue();
        ImCopyPass copyPass = queue.AddCopyPass();
        copyPass.Upload(_vertexBuffer);
        _indexBuffer.UploadData(formatted.Indices.AsSpan, copyPass);
        copyPass.End();
        queue.Submit();
    }

    public unsafe void Render(ImRenderPass renderPass, Vector3 position, Vector2 renderSize)
    {
        if (_empty)
            return;
        
        renderPass.Queue.SetUniforms((new AlignedVector3(position), renderSize));
        
        _shader.Bind(renderPass);
        renderPass.Bind(_vertexBuffer);
        renderPass.Bind(_indexBuffer);
        
        SDL.GPUTextureSamplerBinding binding = new SDL.GPUTextureSamplerBinding
        {
            Texture = _texture,
            Sampler = _sampler
        };
        
        IntPtr ptr = (IntPtr)(&binding);
        
        SDL.BindGPUVertexSamplers(renderPass.Handle, 0, ptr, 1);
        SDL.BindGPUFragmentSamplers(renderPass.Handle, 0, ptr, 1);
        
        renderPass.DrawIndexedPrimatives((uint)_indexBuffer.Length, 1, 0, 0, 0);
    }
}