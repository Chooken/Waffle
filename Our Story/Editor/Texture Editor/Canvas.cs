using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace OurStory.Editor;

public class Canvas
{
    public int Width => _texture.Width;
    public int Height => _texture.Height;
    
    private Texture _texture;
    private GpuTexture _canvasTexture;
    private Buffer<TempPixel> _tempPixels = new (BufferUsage.ComputeStorageRead);
    private ReadWriteTextureBinding[] _readWriteTextureBindings;

    private struct TempPixel
    {
        public Color Color;
        public Vector2 Position;
    }
    
    private struct CanvasBlitUniforms
    {
        public Color SelectedColor;
        public Vector2 CursorPosition;
        public Vector2 TextureSize;
    }

    private Shader _canvasBlitShader;
    private ComputeShader _canvasTempShader;

    public Canvas(uint width, uint height)
    {
        Assert.True(
            Assets.TryGetComputeShader("Core", "canvas-temp-additions", out _canvasTempShader),
            "Canvas Temp Shader not found."
        );
        
        _texture = new Texture(width, height);
        _canvasTexture = new GpuTexture(GpuTextureSettings.Default(width, height) with
        {
            ColorTarget = true,
            RandomWrites = true,
        });

        Assert.True(
            _canvasTexture.TryGetReadWriteBinding(0, out var binding),
            "Couldn't get read write texture binding for canvas.");
        
        _readWriteTextureBindings =
        [
            binding!.Value
        ];
    }

    public Color GetColor(uint x, uint y)
    {
        var data = _texture.GetAs<(byte r, byte g, byte b, byte a)>();

        var color = data[(int)x + (int)y * _texture.Width];

        return Color.RGBA255(color.r, color.g, color.b, color.a);
    }

    public void SetPixel(Color color, uint x, uint y)
    {
        _texture.Data[(int)(x + y * _texture.Width) * 4] =
            color.r255;
        _texture.Data[(int)(x + y * _texture.Width) * 4 + 1] =
            color.g255;
        _texture.Data[(int)(x + y * _texture.Width) * 4 + 2] =
            color.b255;
        _texture.Data[(int)(x + y * _texture.Width) * 4 + 3] =
            color.a255;

        ImQueue queue = new ImQueue();
        ImCopyPass copyPass = queue.AddCopyPass();
        copyPass.Upload(_texture);
        copyPass.End();
        queue.Submit();
    }

    public void SetTempPixel(Color color, Vector2 position)
    {
        _tempPixels.Add(new TempPixel()
        {
            Color = color,
            Position = position
        });
    }

    public void Render(ref ImQueue queue)
    {
        ImCopyPass copyPass = queue.AddCopyPass();
        copyPass.Upload(_tempPixels);
        copyPass.End();
        
        queue.AddBlitPass(_texture, _canvasTexture, true);

        if (_tempPixels.Count != 0)
        {
            ImComputePass computePass = queue.AddComputePass(_readWriteTextureBindings);
            computePass.Bind(_canvasTempShader);
            computePass.Bind(_tempPixels);
            computePass.SetUniforms(_tempPixels.Count);
            computePass.Dispatch(uint.Max(1, (uint)_tempPixels.Count / 64), 1, 1);
            computePass.End();
            
            _tempPixels.Clear();
        }
    }

    public GpuTexture GetCanvas()
    {
        return _canvasTexture;
    }
}