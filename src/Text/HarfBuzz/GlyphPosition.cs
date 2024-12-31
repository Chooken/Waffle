using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

[StructLayout(LayoutKind.Sequential, Size = 20)]
public struct GlyphPosition
{
    public int XAdvance;
    public int YAdvance;
    public int XOffset;
    public int YOffset;
}