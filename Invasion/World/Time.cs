using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invasion.World
{
    public static class Time
    {
        public static float TotalTime => Ticks / 60.0f;
        public static ulong Ticks { get; private set; } = 0;

        public static void Update()
        {
            Ticks++;
        }
    }
}
