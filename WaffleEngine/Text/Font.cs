using SDL3;

namespace WaffleEngine.Text;

public class Font
{
    public IntPtr Handle;

    public int Height => TTF.GetFontHeight(Handle);

    public Font(IntPtr handle)
    {
        Handle = handle;
    }

    public void SetFontHinting(HintingFlags hintingFlags)
    {
        TTF.SetFontHinting(Handle, (TTF.HintingFlags)hintingFlags);
    }

    public static implicit operator IntPtr(Font font) => font.Handle;
}