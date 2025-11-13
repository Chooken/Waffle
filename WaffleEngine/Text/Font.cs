using SDL3;

namespace WaffleEngine.Text;

public class Font
{
    public IntPtr Handle;

    public float Size => TTF.GetFontSize(Handle);
    public int Height => TTF.GetFontHeight(Handle);

    public Font(IntPtr handle)
    {
        Handle = handle;
    }

    public void SetFontHinting(HintingFlags hintingFlags)
    {
        TTF.SetFontHinting(Handle, (TTF.HintingFlags)hintingFlags);
    }

    public void SetFontAlignment()
    {
        TTF.SetFontWrapAlignment(Handle, TTF.HorizontalAlignment.Center);
    }

    public void SetFontSize(float size)
    {
        TTF.SetFontSize(Handle, size);
    }

    public Font Copy()
    {
        return new Font(TTF.CopyFont(Handle));
    }

    public static implicit operator IntPtr(Font font) => font.Handle;
}