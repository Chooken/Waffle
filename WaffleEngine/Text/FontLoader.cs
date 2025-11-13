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

    public static bool TryLoadFont(string name, int pointSize)
    {
        if (_font.ContainsKey($"{name}_{pointSize}"))
            return true;

        string path = $"Assets/{name}";
        
        if (!File.Exists(path))
        {
            WLog.Error($"Font does not exist at: {path}");
            return false;
        }
        
        var font = new Font(TTF.OpenFontIO(SDL.IOFromFile(path, "r"), true, pointSize));
        
        if (font.Handle == IntPtr.Zero)
        {
            WLog.Error("Font failed to load.");
            return false;
        }
        
        font.SetFontHinting(HintingFlags.LightSubpixel);
        _font.Add($"{name}_{pointSize}", font);
        return true;
    }

    public static bool TryGetFont(string name, int pointSize, [NotNullWhen(true)] out Font? font)
    {
        if (_font.TryGetValue($"{name}_{pointSize}", out font))
            return true;
        
        string path = $"Assets/{name}";

        if (!File.Exists(path))
        {
            WLog.Error($"Font does not exist at: {path}");
            return false;
        }
        
        font = new Font(TTF.OpenFontIO(SDL.IOFromFile(path, "r"), true, pointSize));

        if (font.Handle == IntPtr.Zero)
        {
            WLog.Error("Font failed to load.");
            return false;
        }

        font.SetFontHinting(HintingFlags.LightSubpixel);
        _font.Add($"{name}_{pointSize}", font);

        return true;
    }

    public static void Dispose()
    {
        TTF.Quit();
    }
}