using System.Diagnostics;
using System.Threading.Tasks;

namespace Invasion.World
{
    public static class Time
    {
        public static float DeltaTime { get; private set; } = 0.0f;
        public static float TotalTime => Ticks / 60.0f;
        public static ulong Ticks { get; private set; } = 0;
        
        private static bool isRunning = false;

        private static Stopwatch Stopwatch { get; } = new();

        static Time()
        {
            Stopwatch.Start();
            isRunning = true;

            Task.Run(() =>
            {
                while (isRunning)
                    Update();
            });
        }

        private static void Update()
        {
            DeltaTime = (float)Stopwatch.Elapsed.TotalSeconds;
            Stopwatch.Restart();

            Ticks += (ulong)(DeltaTime * 60.0f);
        }

        public static void CleanUp()
        {
            isRunning = false;
        }
    }
}
