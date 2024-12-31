using System.Numerics;
using WaffleEngine.MSDF;
using WaffleEngine.Text.HarfBuzz;

namespace WaffleEngine.Text;

public class MsdfText : IDisposable
{
    public string Text { get; private set; }

    private HBBuffer _buffer;
    private MsdfFont _font;

    public MsdfText(string text, MsdfFont font)
    {
        Text = text;
        _font = font;

        _buffer = new HBBuffer();

        _buffer.AddUtf8(text);

        _buffer.AutoDetectSettings();

        _buffer.Shape(_font);
    }

    public void UpdateText(string text)
    {
        if (text == Text)
            return;

        Text = text;

        _buffer.ClearContents();

        _buffer.AddUtf8(text);

        _buffer.AutoDetectSettings();

        _buffer.Shape(_font);
    }

    public void Render(Camera camera)
    {
        _font.Render(_buffer, camera);
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}