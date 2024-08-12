using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace WaffleEngine;

public class Texture
{
    public int Handle;

    public int Width;
    public int Height;

    private TextureUnit _last_bound_unit;

    public static Texture GetTexture(string folder, string file) => AssetLoader.GetTexture(folder, file);

    public Texture() { }
    public Texture(ref ImageResult image) => GenerateTexture(ref image);

    public Texture(string path)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        
        ImageResult image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

        GenerateTexture(ref image);
    }

    private void GenerateTexture(ref ImageResult image)
    {
        Handle = GL.GenTexture();

        Width = image.Width;
        Height = image.Height;
        
        BindTo(TextureUnit.Texture0);

        var err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Log.Error("OpenGL Error: {0}", err);
        }
        
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexImage2D(
            TextureTarget.Texture2d, 
            0, 
            InternalFormat.Rgba, 
            image.Width, 
            image.Height, 
            0, 
            PixelFormat.Rgba,
            PixelType.UnsignedByte, 
            image.Data);

        err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Log.Error("OpenGL Error: {0}", err);
        }
        else
        {
            Log.Info("Loaded Texture ID: {0}", Handle);
        }
        
        Unbind();
    }

    public void BindTo(TextureUnit texture_unit)
    {
        GL.ActiveTexture(texture_unit);
        GL.BindTexture(TextureTarget.Texture2d, Handle);

        _last_bound_unit = texture_unit;
    }

    public void Unbind()
    {
        GL.ActiveTexture(_last_bound_unit);
        GL.BindTexture(TextureTarget.Texture2d, 0);
    }

    ~Texture()
    {
        if (Handle == -1)
            return;
            
        Log.Error("Texture [ID: {0}]: Wasn't unloaded Before the object got destroyed.");
    }

    public void Unload()
    {
        GL.DeleteTexture(Handle);
        
        Log.Info("Unloaded Texture ID: {0}", Handle);

        Handle = -1;
    }
}