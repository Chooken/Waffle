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
    public static extern bool hb_feature_from_string(byte[] font, int length, out Feature feature);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_feature_to_string(ref Feature feature,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf, uint size);
}