using System.Collections.Generic;

namespace Invasion.Render
{
    public static class ShaderManager
    {
        private static Dictionary<string, Shader> Shaders { get; } = [];

        public static void Register(Shader shader)
        {
            Shaders.Add(shader.Name, shader);
        }

        public static Shader Get(string name)
        {
            return Shaders[name];
        }

        public static void Unregister(string name)
        {
            Shaders[name].CleanUp();
            Shaders.Remove(name);
        }

        public static void CleanUp()
        {
            foreach (var shader in Shaders.Values)
                shader.CleanUp();
        }
    }
}