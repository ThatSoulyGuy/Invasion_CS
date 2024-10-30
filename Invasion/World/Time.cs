using System.Diagnostics;

namespace Invasion.World
{
    public static class Time
    {
        public static float DeltaTime { get; private set; } = 0.0f;
        public static float TotalTime => Ticks / 60.0f;
        public static ulong Ticks { get; private set; } = 0;
        
        private static Stopwatch Stopwatch { get; } = new();

        static Time()
        {
            Stopwatch.Start();
        }

        public static void Update()
        {
            DeltaTime = (float)Stopwatch.Elapsed.TotalSeconds;
            Stopwatch.Restart();

            Ticks += (ulong)(DeltaTime * 60.0f);
        }
    }
}
