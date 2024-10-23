using Invasion.Render;
using System.Collections.Concurrent;

namespace Invasion.ECS
{
    public static class GameObjectManager
    {
        private static ConcurrentDictionary<string, GameObject> GameObjects { get; } = [];

        public static void Register(GameObject gameObject)
        {
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
                if (GameObjects.TryGetValue(name, out GameObject? value))
                {
                    value.CleanUp();
                    
                    GameObjects.TryRemove(name, out _);
                }
            }

        public static void Update()
        {
            foreach (GameObject gameObject in GameObjects.Values)
                gameObject.Update();
        }

        public static void Render(Camera camera)
        {
            foreach (GameObject gameObject in GameObjects.Values)
                gameObject.Render(camera);
        }

        public static void CleanUp()
        {
            foreach (GameObject gameObject in GameObjects.Values)
                gameObject.CleanUp();
            
            GameObjects.Clear();
        }
    }
}
