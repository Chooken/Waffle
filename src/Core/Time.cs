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
        public static double TimeSinceStart = (DateTime.Now - AppStartTime).TotalSeconds;
        public static double DeltaTime = 0;
        public static double UpdateTime = 0;
        public static double AverageFrametime => GetAverageFrametime();
        public static double FramesPerSecond => 1.0 / GetAverageFrametime();

        private const int QUEUE_SIZE = 10; 
        private static Queue<double> _frametimes = new();
        

        public static void AddLastFrameTime(double last_frame_time)
        {
            if (_frametimes.Count >= QUEUE_SIZE)
                _frametimes.Dequeue();
            
            _frametimes.Enqueue(last_frame_time);
        }

        public static void UpdateTimes()
        {
            TimeSinceStart = (DateTime.Now - AppStartTime).TotalSeconds;
        }
        
        private static double GetAverageFrametime()
        {
            if (_frametimes.Count == 0)
                return 1;
            
            double sum = 0;

            foreach (var frametime in _frametimes)
            {
                sum += frametime;
            }

            return sum / _frametimes.Count;
        }
    }
}
