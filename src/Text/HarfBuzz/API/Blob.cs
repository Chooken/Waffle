using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

public static partial class HarfBuzz
{
    public enum MemoryMode
    {
        Duplicate,
        ReadOnly,
        Writable,
        ReadOnlyMayMakeWritable
    }

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_create(byte[] data, uint length, MemoryMode mode, IntPtr user_data, IntPtr destroy);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_create_or_fail(byte[] data, uint length, MemoryMode mode, IntPtr user_data, IntPtr destroy);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_create_from_file(byte[] filename);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_create_from_file_or_fail(byte[] filename);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_create_sub_blob(IntPtr parent, uint offset, uint length);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_copy_writable_or_fail(IntPtr blob);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_get_empty();

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_reference(IntPtr blob);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern void hb_blob_destroy(IntPtr blob);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern  bool hb_blob_set_user_data(IntPtr blob, IntPtr key, IntPtr data, IntPtr destroy, bool replace);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_get_user_data(IntPtr blob, IntPtr key);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern bool hb_blob_make_immutable(IntPtr blob);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern bool hb_blob_is_immutable(IntPtr blob);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern int hb_blob_get_length(IntPtr blob);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_get_data(IntPtr blob, out uint length);

    [DllImport(_harf_buzz_dll, CallingConvention = _call_convention)]
    public static extern IntPtr hb_blob_get_data_writable(IntPtr blob, out uint length);
}