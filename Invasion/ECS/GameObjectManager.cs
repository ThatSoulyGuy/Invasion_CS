using Invasion.Render;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Invasion.ECS
{
    public static class GameObjectManager
    {
        private static ConcurrentDictionary<string, GameObject> GameObjects { get; } = [];

        private static List<GameObject> PendingRegistrations { get; } = [];
        private static List<string> PendingUnregistrations { get; } = [];

        private static bool isEnumerating = false;

        public static void Register(GameObject gameObject)
        {
            if (isEnumerating)
                PendingRegistrations.Add(gameObject);
            else
                GameObjects.TryAdd(gameObject.Name, gameObject);
        }

        public static GameObject Get(string name)
        {
            if (GameObjects.TryGetValue(name, out GameObject? value))
                return value;
            else
                return null!;
        }

        public static void Unregister(string name)
        {
            if (isEnumerating)
                PendingUnregistrations.Add(name);
            else
            {
                if (GameObjects.TryGetValue(name, out GameObject? value))
                {
                    value.CleanUp();
                    
                    GameObjects.TryRemove(name, out _);
                }
            }
        }

        public static void Update()
        {
            isEnumerating = true;

            foreach (GameObject gameObject in GameObjects.Values)
                gameObject.Update();
            
            isEnumerating = false;

            ProcessPendingOperations();
        }

        public static void Render(Camera camera)
        {
            isEnumerating = true;

            foreach (GameObject gameObject in GameObjects.Values)
                gameObject.Render(camera);

            isEnumerating = false;

            ProcessPendingOperations();
        }

        public static void CleanUp()
        {
            foreach (GameObject gameObject in GameObjects.Values)
                gameObject.CleanUp();
            
            GameObjects.Clear();
        }

        private static void ProcessPendingOperations()
        {
            foreach (var gameObject in PendingRegistrations)
                GameObjects.TryAdd(gameObject.Name, gameObject);
            
            PendingRegistrations.Clear();

            foreach (var name in PendingUnregistrations)
            {
                if (GameObjects.TryGetValue(name, out GameObject? value))
                {
                    value.CleanUp();
                    GameObjects.TryRemove(name, out _);
                }
            }

            PendingUnregistrations.Clear();
        }
    }
}
