using SDL3;
using WaffleEngine.Native;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Text;

public class AtlasedText
{
    public IntPtr Handle;
    
    private bool _empty;
    private int _fontSize;
    
    private static IntPtr _textEngine;
    private static Shader? _shader;

    private Buffer<Vertex> _vertexBuffer = new Buffer<Vertex>(BufferUsage.Vertex);
    private Buffer<int> _indexBuffer = new Buffer<int>(BufferUsage.Index);

    private IntPtr _texture;
    private IntPtr _sampler;
    private Color _color;

    public AtlasedText(string text, Font font, Color color)
    {
        _color = color;
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
    }

    public Vector2 GetSize()
    {
        TTF.GetTextSize(Handle, out int w, out int h);
        return new Vector2(w, h);
    }

    public void SetFont(Font font)
    {
        TTF.SetTextFont(Handle, font.Handle);
    }

    public void SetColor(Color color)
    {
        _color = color;
        TTF.SetTextColorFloat(Handle, color.r, color.g, color.b, color.a);
    }

    public void SetText(string text)
    {
        TTF.SetTextString(Handle, text, (uint)text.Length);
    }

    public void SetWrapWidth(int width)
    {
        TTF.SetTextWrapWidth(Handle, Math.Max(width, 0));
    }
    
    private struct GPUAtlasDrawSequenceFormatted
    {
        public IntPtr AtlasTexture;
        public NativeArray<Vector2> Vertices;
        public NativeArray<Vector2> UVs;
        public NativeArray<int> Indices;
        public TTF.ImageType ImageType;
        public NativePtr<TTF.GPUAtlasDrawSequence> Next;

        public static GPUAtlasDrawSequenceFormatted From(TTF.GPUAtlasDrawSequence sequence)
        {
            return new GPUAtlasDrawSequenceFormatted()
            {
                AtlasTexture = sequence.AtlasTexture,
                Vertices = new NativeArray<Vector2>(sequence.XY, (uint)sequence.NumVertices),
                UVs = new NativeArray<Vector2>(sequence.UV, (uint)sequence.NumVertices),
                Indices = new NativeArray<int>(sequence.Indices, (uint)sequence.NumIndices),
                ImageType = sequence.ImageType,
                Next = sequence.Next,
            };
        }
    }

    public unsafe void Update()
    {
        NativePtr<TTF.GPUAtlasDrawSequence> sequence = TTF.GetGPUTextDrawData(Handle);

        if (sequence.IsNull)
        {
            WLog.Error($"{SDL.GetError()}");
            _empty = true;
            return;
        }

        _vertexBuffer.Clear();
        _indexBuffer.Clear();
        _texture = IntPtr.Zero;

        // SDL hands us y-up vertices, origin at the text's top-left with the
        // body extending into negative Y. Our UI space is y-down, so mirror
        // positions here and keep ui-mesh y-down-native. UVs are passed
        // through untouched: they address rows of the atlas texture as
        // stored, so flipping them samples empty texels (invisible text).
        // (SDL docs: "positive Y upwards ... transform the vertices yourself".)
        for (var seq = sequence; !seq.IsNull; seq = seq.Value.Next)
        {
            var formatted = GPUAtlasDrawSequenceFormatted.From(seq.Value);

            // Solid-fill sequences (underline/strikethrough backings) have no
            // atlas/UVs and need a different pipeline — skip them for now.
            if (formatted.AtlasTexture == IntPtr.Zero)
                continue;

            if (_texture == IntPtr.Zero)
                _texture = formatted.AtlasTexture;

            int vertexBase = _vertexBuffer.Count;

            for (int i = 0; i < formatted.Vertices.Length; i++)
            {
                var pos = formatted.Vertices[i];
                var uv = formatted.UVs[i];
                _vertexBuffer.Add(new Vertex()
                {
                    Color = _color,
                    Position = new Vector4(pos.x, -pos.y, 0, 1),
                    Uv = new Vector2(uv.x, uv.y),
                });
            }

            for (int i = 0; i < formatted.Indices.Length; i++)
                _indexBuffer.Add(vertexBase + formatted.Indices[i]);
        }

        if (_vertexBuffer.Count == 0 || _indexBuffer.Count == 0)
        {
            _empty = true;
            return;
        }

        _empty = false;

        ImQueue queue = new ImQueue();
        ImCopyPass copyPass = queue.AddCopyPass();
        copyPass.Upload(_vertexBuffer);
        copyPass.Upload(_indexBuffer);
        copyPass.End();
        queue.Submit();
    }

    public unsafe void Render(
        ImRenderPass renderPass, Vector2 position, Vector2 renderSize,
        Vector2 clipMin, Vector2 clipMax)
    {
        if (_shader is null)
        {
            if (!Assets.TryGetShader("builtin", "ui-mesh", out _shader))
            {
                WLog.Error("Shader not found: builtin/ui-mesh");
                return;
            }
        }
        
        if (_empty || _vertexBuffer.Count == 0)
            return;

        renderPass.SetUniforms(UiMeshData.Text(position, renderSize, clipMin, clipMax));

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
        
        renderPass.DrawIndexedPrimatives((uint)_indexBuffer.Count, 1, 0, 0, 0);
    }
}