using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaffleEngine.Text.HarfBuzz;

namespace WaffleEngine.Text.HarfBuzz;

public class HBFont : IDisposable
{
    /// <inheritdoc />
    public HBFont(IntPtr handle)
    {
        Handle = handle;
    }

    public HBFont(HBFace face)
    {
        Handle = HarfBuzz.hb_font_create(face.Handle);
    }

    /// <summary>
    ///     The pointer to the unmanaged hb_font_t object.
    /// </summary>
    public IntPtr Handle { get; }

    public (int x, int y) Scale
    {
        get
        {
            HarfBuzz.hb_font_get_scale(Handle, out var x, out var y);
            return (x, y);
        }
        set => HarfBuzz.hb_font_set_scale(Handle, value.x, value.y);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        HarfBuzz.hb_font_destroy(Handle);
    }
}