using Invasion.Math;
using Invasion.Render;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Invasion.ECS
{
    public class GameObject
    {
        public string Name { get; set; } = string.Empty;

        public Transform Transform => GetComponent<Transform>();

        public bool Active { get; set; } = true;

        private GameObject? Parent { get; set; }

        private ConcurrentDictionary<Type, Component> Components { get; } = [];
        private ConcurrentDictionary<string, GameObject> Children { get; } = [];

        private static int MaxThreads { get; } = Environment.ProcessorCount / 2;
        private static BlockingCollection<Action> TaskQueue { get; } = [];
        private static List<Thread> ThreadPool { get; } = [];
        private static bool ThreadPoolInitialized = false;
        private static object ThreadPoolLock { get; } = new();

        private object Lock { get; } = new();

        private GameObject()
        {
            if (ThreadPoolInitialized)
                return;

            lock (ThreadPoolLock)
            {
                if (ThreadPoolInitialized)
                    return;

                for (int i = 0; i < MaxThreads; i++)
                {
                    Thread worker = new(() =>
                    {
                        foreach (var action in TaskQueue.GetConsumingEnumerable())
                        {
                            try
                            {
                                action();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Exception in thread pool: {ex.Message}");
                            }
                        }
                    })
                    {
                        IsBackground = true
                    };
                    worker.Start();
                    ThreadPool.Add(worker);
                }

                ThreadPoolInitialized = true;
            }
        }

        public T AddComponent<T>(T component) where T : Component
        {
            component.GameObject = this;
            component.Initialize();
            Components.TryAdd(typeof(T), component);

            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            Components.TryGetValue(typeof(T), out var component);
            return (T)component!;
        }

        public bool HasComponent<T>() where T : Component
        {
            return Components.ContainsKey(typeof(T));
        }

        public void RemoveComponent<T>() where T : Component
        {
            if (Components.TryGetValue(typeof(T), out var component))
            {
                component.CleanUp();
                component = null!;
                Components.TryRemove(typeof(T), out _);
            }
        }

        public GameObject AddChild(GameObject child)
        {
            GameObjectManager.Unregister(child.Name, false);

            child.Parent = this;
            child.GetComponent<Transform>().Parent = GetComponent<Transform>();

            Children.TryAdd(child.Name, child);

            return Children[child.Name];
        }

        public GameObject GetChild(string name)
        {
            return Children[name];
        }

        public void RemoveChild(string name)
        {
            if (Children.TryGetValue(name, out var child))
            {
                child.Parent = null;
                child.GetComponent<Transform>().Parent = null;

                GameObjectManager.Register(child);

                Children.TryRemove(name, out _);
            }
        }

        public GameObject? GetParent()
        {
            return Parent;
        }

        public void OnCollide(GameObject other)
        {
            lock (Lock)
            {
                if (!Active)
                    return;

                foreach (var component in Components.Values)
                    component.OnCollide(other);
            }
        }

        public void Update()
        {
            
            if (!Active)
                return;

            foreach (var component in Components.Values)
                lock (Lock) { TaskQueue.Add(component.Update); }

            foreach (var child in Children.Values)
                lock (Lock) { TaskQueue.Add(child.Update); }
        }

        public void Render(Camera camera)
        {
            if (!Active)
                return;

            foreach (Component component in Components.Values)
                component.Render(camera);

            foreach (GameObject child in Children.Values)
                child.Render(camera);
        }

        public void CleanUp()
        {
            foreach (Component component in Components.Values)
                component.CleanUp();

            Components.Clear();

            foreach (GameObject child in Children.Values)
                child.CleanUp();

            Children.Clear();
        }

        public static GameObject Create(string name)
        {
            GameObject result = new() 
            {
                Name = name
            };

            result.AddComponent(Transform.Create());

            GameObjectManager.Register(result);

            return result;
        }

        public static void ShutdownThreadPool()
        {
            TaskQueue.CompleteAdding();

            foreach (var thread in ThreadPool)
                thread.Join();
        }
    }
}
