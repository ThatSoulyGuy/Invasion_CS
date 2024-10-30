using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Invasion.Thread
{
    public class TaskScheduler
    {
        private BlockingCollection<Action> TaskQueue { get; } = [];
        private List<System.Threading.Thread> Threads { get; set; }

        private TaskScheduler(int numberOfThreads)
        {
            Threads = Enumerable.Range(0, numberOfThreads).Select(_ => new System.Threading.Thread(Worker)).ToList();
        }

        public static TaskScheduler Create()
        {
            return Create(Environment.ProcessorCount);
        }

        public static TaskScheduler Create(int numberOfThreads)
        {
            TaskScheduler result = new(numberOfThreads);

            result.Initialize();

            return result;
        }

        public void Schedule(Action task)
        {
            if (!TaskQueue.IsAddingCompleted)
                TaskQueue.Add(task);
        }

        private void Initialize()
        {
            foreach (var thread in Threads)
                thread.Start();
        }

        private void Worker()
        {
            foreach (var task in TaskQueue.GetConsumingEnumerable())
                task?.Invoke();
        }

        public void CleanUp()
        {
            TaskQueue.CompleteAdding();

            foreach (var thread in Threads)
                thread.Join();

            TaskQueue.Dispose();
        }
    }
}
