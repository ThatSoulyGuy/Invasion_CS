using Invasion.Render;
using System.Collections.Generic;

namespace Invasion.ECS
{
    public static class GameObjectManager
    {
        private static Dictionary<string, GameObject> GameObjects { get; } = new();

        public static void Register(GameObject gameObject)
        {
            GameObjects.Add(gameObject.Name, gameObject);
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
            GameObjects[name].CleanUp();
            GameObjects.Remove(name);
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
        }
    }
}
