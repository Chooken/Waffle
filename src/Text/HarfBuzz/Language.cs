using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine.Text.HarfBuzz;

public class Language
{
    public static readonly Language Default = new Language(HarfBuzz.hb_language_get_default());

    public IntPtr Handle { get; private set; }

    public Language(IntPtr handle)
    {
        Handle = handle;
    }
}