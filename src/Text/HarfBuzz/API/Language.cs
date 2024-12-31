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
    public static extern IntPtr hb_language_from_string(byte[] str, int length);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_language_get_default();
}