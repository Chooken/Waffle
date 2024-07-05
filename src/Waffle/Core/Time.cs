using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine
{
    public static class Time
    {
        public static DateTime AppStartTime = DateTime.Now;
        public static float TimeSinceStart => (float)(DateTime.Now - AppStartTime).TotalSeconds;
    }
}
