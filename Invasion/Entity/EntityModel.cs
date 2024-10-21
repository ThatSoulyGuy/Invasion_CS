using Invasion.ECS;
using Invasion.Render;
using Invasion.World;
using System;
using System.Collections.Generic;

namespace Invasion.Entity.Model
{
    public abstract class EntityModel : Component
    {
        public GameObject ModelObject => GameObject.GetChild("Model");

        private Dictionary<string, GameObject> Parts { get; } = [];

        protected EntityModel() { }

        public override void Initialize()
        {
            GameObject.AddChild(GameObject.Create("Model"));
        }

        public ModelPart Register(ModelPart part)
        {
            if (Parts.ContainsKey(part.Name))
                return null!;

            string name = part.Name;

            ModelObject.AddChild(GameObject.Create(name));

            GameObject partObject = ModelObject.GetChild(name);

            partObject.AddComponent(ShaderManager.Get("default"));
            partObject.AddComponent(TextureAtlasManager.Get("entities").Atlas);
            partObject.AddComponent(TextureAtlasManager.Get("entities"));
            partObject.AddComponent(UIMesh.Create(part.Name + "_Mesh_" + new Random().Next(), [], []));
            ModelPart addedPart = partObject.AddComponent(part);
             
            Parts.Add(part.Name, partObject);

            return addedPart;
        }

        public ModelPart GetPart(string name)
        {
            if (Parts.TryGetValue(name, out GameObject? value))
                return value.GetComponent<ModelPart>();

            return null!;
        }

        public void Unregister(string name) 
        {
            if (Parts.TryGetValue(name, out GameObject? value))
            {
                GameObjectManager.Unregister(value.Name);
                Parts.Remove(name);
            }
        }
    }
}
