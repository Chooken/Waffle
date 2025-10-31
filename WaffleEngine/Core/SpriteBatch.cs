using System.Numerics;
using System.Runtime.InteropServices;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public class SpriteBatch
{
    private Dictionary<Texture, Buffer<SpriteInstanceData>> _batches = new ();

    private static Shader? _spriteShader = null;

    public static bool Init()
    {
        if (!Assets.TryGetShader("builtin", "sprite", out _spriteShader))
        {
            WLog.Error("Failed to load sprite shader.");
            return false;
        }

        return true;
    }

    public void AddSprite(Sprite sprite, Transform transform)
    {
        if (!_batches.TryGetValue(sprite.Texture, out var buffer))
        {
            buffer = new Buffer<SpriteInstanceData>(BufferUsage.GraphicsStorageRead);
            _batches.Add(sprite.Texture, buffer);
        }
        
        buffer.Add(sprite.GetInstanceData(transform));
    }

    public void Upload(ImCopyPass copyPass)
    {
        foreach (var buffer in _batches.Values)
        {
            buffer.UploadToGpu(copyPass);
        }
    }

    public void Render(ImRenderPass renderPass)
    {
        if (_spriteShader is null)
            return;
        
        foreach ((Texture texture, var buffer) in _batches)
        {
            renderPass.Bind(_spriteShader);
            renderPass.Bind(buffer);
            renderPass.Bind(texture, 0);
            renderPass.DrawPrimatives(6, (uint)buffer.Count, 0, 0);
            buffer.Clear();
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SpriteInstanceData
{
    public Matrix4x4 ModelMatrix;
    public Vector2 Size;
    public Vector2 p;
}

public struct Sprite
{
    public Texture Texture;
    public Shader SpriteShader;
    public bool ScaleToOneUnit;
    public int PixelsPerUnit;

    public SpriteInstanceData GetInstanceData(Transform transform)
    {
        Vector2 size;

        bool widthLonger = Texture.Width > Texture.Height;

        if (widthLonger)
        {
            size = new Vector2(1, (float)Texture.Height / Texture.Width);
        }
        else
        {
            size = new Vector2((float)Texture.Width / Texture.Height, 1);
        }

        return new SpriteInstanceData()
        {
            ModelMatrix = transform.TransformationMatrix,
            Size = (ScaleToOneUnit)
                ? size
                : new Vector2((float)Texture.Width / PixelsPerUnit, (float)Texture.Height / PixelsPerUnit)
        };
    }
}