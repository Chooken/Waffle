using SDL3;
using WaffleEngine.Rendering;

namespace WaffleEngine.Text;

public class Text
{
    public IntPtr Handle;
    
    private static IntPtr _textEngine;


    public Text(string text, Font font, Color color)
    {
        if (_textEngine == IntPtr.Zero)
        {
            _textEngine = TTF.CreateGPUTextEngine(Device.Handle);
        }
        
        Handle = TTF.CreateText(_textEngine, font.Handle, text, (uint)text.Length);
        TTF.SetTextColor(Handle, color.r255, color.g255, color.b255, color.a255);
    }

    public void SetColor(Color color)
    {
        TTF.SetTextColorFloat(Handle, color.r, color.g, color.b, color.a);
    }

    public void SetText(string text)
    {
        TTF.SetTextString(Handle, text, (uint)text.Length);
    }
}