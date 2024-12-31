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
    public static extern IntPtr hb_font_create(IntPtr face);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_font_destroy(IntPtr font);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_font_set_scale(IntPtr font, int x, int y);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_font_get_scale(IntPtr font, out int x, out int y);
}