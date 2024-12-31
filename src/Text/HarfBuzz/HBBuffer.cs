using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

public class HBBuffer : IDisposable
{
    private IntPtr _handle;

    public Direction Direction 
    { 
        set { HarfBuzz.hb_buffer_set_direction(_handle, value); } 
    }

    public Tag Script 
    { 
        set { HarfBuzz.hb_buffer_set_script(_handle, (int)value.Value); } 
    }

    public string Language
    {
        set
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            HarfBuzz.hb_buffer_set_language(_handle, HarfBuzz.hb_language_from_string(bytes, bytes.Length));
        }
    }

    /// <summary>
    ///     Buffer glyph information array. Returned pointer is valid as long as buffer contents are not modified.
    /// </summary>
    public unsafe Span<GlyphInfo> GlyphInfos =>
        new Span<GlyphInfo> ((void*)HarfBuzz.hb_buffer_get_glyph_infos(_handle, out var length), (int) length);

    /// <summary>
    ///     Buffer glyph position array. Returned pointer is valid as long as buffer contents are not modified.
    /// </summary>
    public unsafe Span<GlyphPosition> GlyphPositions =>
         new Span<GlyphPosition>((void*)HarfBuzz.hb_buffer_get_glyph_positions(_handle, out var length), (int) length);

    public HBBuffer()
    {
        _handle = HarfBuzz.hb_buffer_create();
    }

    public void AddUtf8(byte[] text, uint item_offset = 0, int item_length = -1)
    {
        HarfBuzz.hb_buffer_add_utf8(_handle, text, text.Length, item_offset, item_length > -1 ? item_length : text.Length);
    }

    public void AddUtf8(string text, uint item_offset = 0, int item_length = -1)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        AddUtf8(bytes, item_offset, item_length);
    }

    public void Shape(HBFont font, params Feature[] features)
    {
        HarfBuzz.hb_shape(font.Handle, _handle, features, (uint)features.Length);
    }

    public void AutoDetectSettings()
    {
        HarfBuzz.hb_buffer_guess_segment_properties(_handle);
    }

    /// <summary>
    ///     Resets the buffer to its initial status, as if it was just newly created with the constructor.
    /// </summary>
    public void Reset()
    {
        HarfBuzz.hb_buffer_reset(_handle);
    }

    /// <summary>
    ///     Similar to <see cref="Reset" />, but does not clear the Unicode functions and the replacement code point.
    /// </summary>
    public void ClearContents()
    {
        HarfBuzz.hb_buffer_clear_contents(_handle);
    }

    public void Dispose()
    {
        HarfBuzz.hb_buffer_destroy(_handle);
    }
}