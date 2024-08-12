using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine;

public static class RenderTarget
{
    public static void Set(RenderTexture texture)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, texture.FrameBuffer);
        GL.Viewport(0,0,texture.Width,texture.Height);
    }

    public static void Screen()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0,0, Window.Width, Window.Height);
    }

    public static void Clear()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
}