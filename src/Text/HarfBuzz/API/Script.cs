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
    public static extern int hb_script_from_string(byte[] buffer, int length);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern int hb_script_from_iso15924_tag(int tag);

}