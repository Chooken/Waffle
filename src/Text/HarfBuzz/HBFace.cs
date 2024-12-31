using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

public class HBFace : IDisposable
{
    /// <inheritdoc />
    public HBFace(HBBlob blob, uint index = 0)
    {
        Handle = HarfBuzz.hb_face_create(blob.Handle, index);
    }

    /// <summary>
    ///     The pointer to the unmanaged hb_face_t object.
    /// </summary>
    public IntPtr Handle { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        HarfBuzz.hb_face_destroy(Handle);
    }
}