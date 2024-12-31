using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

[StructLayout(LayoutKind.Sequential)]
public struct Feature
{
    private uint _tag;
    public uint Value;
    public uint Start;
    public uint End;

    /// <summary>
    ///     Parses a string into a <see cref="Feature" />.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public Feature(string name)
    {
        var bytes = Encoding.ASCII.GetBytes(name);
        if (!HarfBuzz.hb_feature_from_string(bytes, bytes.Length, out this)) throw new ArgumentException(nameof(name));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        const int allocatedSize = 128;
        var buffer = new StringBuilder(allocatedSize);
        HarfBuzz.hb_feature_to_string(ref this, buffer, allocatedSize);
        return buffer.ToString();
    }

    public Tag Tag
    {
        get => new Tag { Value = _tag };
        set => _tag = value.Value;
    }
}
