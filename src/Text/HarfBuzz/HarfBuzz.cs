using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

public static partial class HarfBuzz
{
    private const string _harf_buzz_dll = "libharfbuzz-0.dll";
    private const CallingConvention _call_convention = CallingConvention.Cdecl;
}
