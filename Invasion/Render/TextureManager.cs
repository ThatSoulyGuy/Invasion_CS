using System;
using System.Collections.Generic;

namespace Invasion.Render
{
    public static class TextureManager
    {
        private static Dictionary<string, Texture> Textures { get; } = new();

        public static void Register(Texture texture)
        {
            Textures.Add(texture.Name, texture);
        }

        public static Texture Get(string name)
        {
            return Textures[name];
        }

        public static void Unregister(string name)
        {
            Textures[name].CleanUp();
            Textures.Remove(name);
        }

        public static void CleanUp()
        {
            foreach (var texture in Textures.Values)
                texture.CleanUp();
        }
    }
}
