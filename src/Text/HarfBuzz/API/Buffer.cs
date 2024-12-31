using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

public static partial class HarfBuzz
{
    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_buffer_create();

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_destroy(IntPtr buffer);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_set_direction(IntPtr buffer, Direction direction);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_set_script(IntPtr buffer, int script);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_get_length(IntPtr buffer);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_add_utf8(IntPtr buffer, byte[] text, int text_length, uint item_offset, int item_length);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_set_language(IntPtr buffer, IntPtr language);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_buffer_get_glyph_infos(IntPtr buffer, out uint length);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_buffer_get_glyph_positions(IntPtr buffer, out uint length);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_guess_segment_properties(IntPtr buffer);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_reset(IntPtr buffer);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_buffer_clear_contents(IntPtr buffer);


}