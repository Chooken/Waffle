using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

[StructLayout(LayoutKind.Sequential, Size = 20)]
public struct GlyphInfo
{
    public uint Codepoint;
    public uint Mask;
    public uint Cluster;
}