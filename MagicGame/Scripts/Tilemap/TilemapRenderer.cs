using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace MagicGame.Scripts;

public static class TilemapRenderer
{
    private static List<Buffer<GpuTile>> _renderQueue = new ();

    private static Color _clearColor;
    private static Texture? _tilesheet;
    private static Texture? _palette;
    private static Shader? _shader;

    public static Color ClearColor => _clearColor;

    public static bool Init()
    {
        if (!Assets.TryGetTexture("core", "tilesheet", out _tilesheet))
        {
            return false;
        }
        
        if (!Assets.TryGetTexture("core", "palette", out _palette))
        {
            return false;
        }
        
        if (!Assets.TryGetShader("core", "tile", out _shader))
        {
            return false;
        }
        
        _clearColor = _palette.GetAs<Color255>()[8];
        
        return true;
    }

    public static void QueueTiles(Buffer<GpuTile> buffer) => _renderQueue.Add(buffer);

    public static void CopyTiles(ImCopyPass copyPass)
    {
        foreach (var buffer in _renderQueue)
        {
            copyPass.Upload(buffer);
        }
    }
    
    public static void RenderTiles(ImRenderPass renderPass, Camera camera)
    {
        if (_shader is null || _tilesheet is null || _palette is null)
            return;
        
        renderPass.SetUniforms(camera.GetProjectionMatrix());
        
        renderPass.Bind(_shader);
        renderPass.Bind(_tilesheet, 0);
        renderPass.Bind(_palette, 1);

        foreach (var buffer in _renderQueue)
        {
            if (buffer.Count == 0)
                continue;
            
            renderPass.Bind(buffer);
            renderPass.DrawPrimatives(6, (uint)buffer.Count, 0, 0);
        }
        
        _renderQueue.Clear();
    }
}