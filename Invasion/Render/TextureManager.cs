using System;
using System.Collections.Generic;

namespace Invasion.Render
{
    public static class TextureManager
    {
        private static Dictionary<string, dynamic> Textures { get; } = new();

        public static void Register<T>(T texture)
        {
            if (texture is not Texture && texture is not TextureCube)
                throw new ArgumentException("Texture must be of type Texture or TextureCube");

            Textures.Add(((dynamic)texture).Name, texture);
        }

        public static T Get<T>(string name)
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
