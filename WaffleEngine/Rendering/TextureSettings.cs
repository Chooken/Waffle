namespace WaffleEngine.Rendering;

public struct GpuTextureSettings
{
    public uint Width;
    public uint Height;
    public TextureFormat Format;
    public bool ColorTarget;
    public bool RandomWrites;
    public FilterMode MinFilter;
    public FilterMode MagFilter;
    public FilterMode MipsFilter;
    public GPUSamplerMode SamplerMode;

    public static GpuTextureSettings Default(uint width, uint height) => new GpuTextureSettings()
    {
        Width = width,
        Height = height,
        Format = TextureFormat.B8G8R8A8Unorm,
        ColorTarget = false,
        RandomWrites = false,
        MinFilter = FilterMode.Linear,
        MagFilter = FilterMode.Linear,
        MipsFilter = FilterMode.Nearest,
        SamplerMode = GPUSamplerMode.ClampToEdge,
    };
    
    public static GpuTextureSettings FromWindow(Window window, bool randomWrite = false) => new GpuTextureSettings()
    {
        Width = (uint)window.Width,
        Height = (uint)window.Height,
        Format = window.GetSwapchainTextureFormat(),
        ColorTarget = true,
        RandomWrites = randomWrite,
        MinFilter = FilterMode.Linear,
        MagFilter = FilterMode.Linear,
        MipsFilter = FilterMode.Nearest,
        SamplerMode = GPUSamplerMode.ClampToEdge,
    };
}

public enum FilterMode
{
    /// <summary>
    /// Point filtering.
    /// </summary>
    Nearest,
        
    /// <summary>
    /// Linear filtering.
    /// </summary>
    Linear
}

public enum GPUSamplerMode
{
    /// <summary>
    /// Specifies that the coordinates will wrap around.
    /// </summary>
    Repeat,
        
    /// <summary>
    /// Specifies that the coordinates will wrap around mirrored.
    /// </summary>
    MirroredRepeat,
        
    /// <summary>
    /// Specifies that the coordinates will clamp to the 0-1 range.
    /// </summary>
    ClampToEdge
}