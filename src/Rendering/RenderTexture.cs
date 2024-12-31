using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public class RenderTexture : Texture
{
    public int DepthBuffer { get; private set; }

    public int FrameBuffer { get; private set; }

    public RenderTexture(int width, int height)
    {
        FrameBuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);

        Handle = GL.GenTexture();

        Width = width; 
        Height = height;

        GL.BindTexture(TextureTarget.Texture2d, Handle);

        unsafe
        {
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        }

        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

        DepthBuffer = GL.GenRenderbuffer();

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthBuffer);

        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent, width, height);

        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthBuffer);

        GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, Handle, 0);

        GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

        FramebufferStatus error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (error != FramebufferStatus.FramebufferComplete)
        {
            Log.Error("Framebuffer Error: {0}", error);
        }
        else 
        {
            Log.Info("Loaded Texture ID: {0} [RenderTexture]", Handle);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void UpdateRenderTexture(int width, int height)
    {
        Width = width;
        Height = height;

        GL.BindTexture(TextureTarget.Texture2d, Handle);

        unsafe
        {
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        }

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthBuffer);

        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent, width, height);
    }
}
