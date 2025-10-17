using System.Diagnostics.CodeAnalysis;
using SDL3;
using WaffleEngine.Rendering;

namespace WaffleEngine.Text;

public static class FontLoader
{
    private static bool _initialised = false;
    private static Dictionary<string, Font> _font = new();
    private static IntPtr _textEngine;

    public static void Init()
    {
        if (_initialised) return;
        
        TTF.Init();
        _initialised = true;

        _textEngine = TTF.CreateGPUTextEngine(Device.Handle);
    }

    public static void LoadFont(string name, int pointSize)
    {
        if (_font.ContainsKey(name))
            return;
        
        var font = new Font(TTF.OpenFontIO(SDL.IOFromFile(name, "r"), true, pointSize));
        font.SetFontHinting(HintingFlags.LightSubpixel);
        _font.Add(name, font);
    }

    public static Font GetFont(string name, int pointSize)
    {
        if (_font.TryGetValue(name, out var font))
            return font;
        
        font = new Font(TTF.OpenFontIO(SDL.IOFromFile(name, "r"), true, pointSize));
        font.SetFontHinting(HintingFlags.LightSubpixel);
        _font.Add(name, font);

        return font;
    }

    public static unsafe bool TryRenderText(string text, string fontName, Color foregoundColor, Color backgroundColor, int widthInPixels, [NotNullWhen(true)] out Texture? texture)
    {
        texture = null;
        
        if (!_font.TryGetValue(fontName, out var font))
        {
            return false;
        }
        

        // var surface = TTF.RenderTextLCDWrapped(font, text, 0, foregoundColor, backgroundColor, widthInPixels);
        //
        // texture = new Texture(surface);

        return true;
    }
    
    public static unsafe bool TryRenderTextToTexture(string text, string fontName, Color foregoundColor, Color backgroundColor, int widthInPixels, ref Texture? texture)
    {
        if (!_font.TryGetValue(fontName, out var font))
        {
            return false;
        }
        
        IntPtr textHandle = TTF.CreateText(_textEngine, font, text, 0);

        TTF.GPUAtlasDrawSequenceFormatted drawFormatted = ((TTF.GPUAtlasDrawSequence*)TTF.GetGPUTextDrawData(textHandle))->AsFormatted();

        var surface = TTF.RenderTextLCDWrapped(font, text, 0, foregoundColor, backgroundColor, widthInPixels);
        
        if (texture is null)
        {
            texture = new Texture(surface);
        }
        else
        {
            texture.SetSurface(surface);
        }

        return true;
    }

    public static void Dispose()
    {
        TTF.Quit();
    }
}