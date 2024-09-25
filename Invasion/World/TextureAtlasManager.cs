using System;
using System.Collections.Generic;

namespace Invasion.World
{
    public static class TextureAtlasManager
    {
        private static Dictionary<string, TextureAtlas> TextureAtlases { get; } = [];

        public static void Register(TextureAtlas textureAtlas)
        {
            if (TextureAtlases.ContainsKey(textureAtlas.Name))
                throw new ArgumentException($"Texture atlas '{textureAtlas.Name}' already registered.");

            TextureAtlases.Add(textureAtlas.Name, textureAtlas);
        }

        public static TextureAtlas Get(string name)
        {
            if (TextureAtlases.TryGetValue(name, out TextureAtlas? textureAtlas))
                return textureAtlas;

            throw new ArgumentException($"Texture atlas '{name}' not found.");
        }

        public static void Unregister(string name)
        {
            if (!TextureAtlases.ContainsKey(name))
                throw new ArgumentException($"Texture atlas '{name}' not found.");

            TextureAtlases.Remove(name);
        }

        public static void Cleanup()
        {
            foreach (var textureAtlas in TextureAtlases.Values)
                textureAtlas.CleanUp();

            TextureAtlases.Clear();
        }
    }
}
