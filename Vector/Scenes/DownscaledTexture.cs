using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace Vector.Scenes;

public class DownscaledTexture
{
    private GpuTexture? _fullGpuTexture;
    private GpuTexture? _gpuTexture;

    public enum ScaleMode
    {
        Width,
        Height
    }

    private ScaleMode _scaleMode;
    private uint _target;

    public DownscaledTexture(ScaleMode scale_mode, uint target)
    {
        _scaleMode = scale_mode;
        _target = target;
    }

    public void SetOutputTexture(GpuTexture gpu_texture)
    {
        _fullGpuTexture = gpu_texture;
    }

    public void GetDimensions(out uint width, out uint height)
    {
        switch (_scaleMode)
        {
            case ScaleMode.Width:
                width = _target;
                float h_ratio = (float)_fullGpuTexture.Height / _fullGpuTexture.Width;
                height = (uint)(width * h_ratio);
                return;
            case ScaleMode.Height:
                height = _target;
                float w_ratio = (float)_fullGpuTexture.Width / _fullGpuTexture.Height;
                width = (uint)(height * w_ratio);
                return;
            default:
                width = 0;
                height = 0;
                return;
        }
    }

    public GpuTexture GetTexture()
    {
        if (_fullGpuTexture == null)
        {
            Log.Error("No destination texture set.");
            throw new NullReferenceException();
        }
        
        GetDimensions(out uint width, out uint height);

        if (_gpuTexture == null)
        {
            _gpuTexture = new GpuTexture(GpuTextureSettings.Default(width, height) with
            {
                ColorTarget = true,
                Format = _fullGpuTexture.Format,
                MagFilter = FilterMode.Nearest,
                MinFilter = FilterMode.Nearest,
                MipsFilter = FilterMode.Nearest,
            });
        }
        
        if (_gpuTexture.Width != width ||
            _gpuTexture.Height != height)
        {
            _gpuTexture.Resize(width, height);
        }

        return _gpuTexture;
    }

    public GpuTexture GetFullResTexture(ImQueue queue)
    {
        queue.AddBlitPass(_gpuTexture, _fullGpuTexture, true);
        return _fullGpuTexture;
    }
}