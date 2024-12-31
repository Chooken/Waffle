using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

public class HBBlob : IDisposable
{
    public IntPtr Handle { get; }
    public int Length { get; }

    public HBBlob(byte[] data)
    {
        Handle = HarfBuzz.hb_blob_create(data, (uint)data.Length, HarfBuzz.MemoryMode.ReadOnly,
            IntPtr.Zero, IntPtr.Zero);
        Length = data.Length;
    }

    public byte[] GetData()
    {
        var ptr = HarfBuzz.hb_blob_get_data(Handle, out var length);
        var data = new byte[length];
        Marshal.Copy(ptr, data, 0, (int)length);
        return data;
    }

    public void Dispose()
    {
        HarfBuzz.hb_blob_destroy(Handle);
    }
}