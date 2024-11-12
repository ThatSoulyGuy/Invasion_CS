using Invasion.ECS;
using Invasion.Math;
using Invasion.Render;
using Invasion.World;
using System;
using System.Collections.Generic;

namespace Invasion.Entity.Model
{
    public abstract class EntityModel : Component
    {
        public GameObject ModelObject => GameObject.GetChild("Model");

        private Dictionary<string, GameObject> Parts { get; } = new();
        private Dictionary<string, GameObject> DamageMeshes { get; } = new();

        protected EntityModel() { }

        public override void Initialize()
        {
            GameObject.AddChild(GameObject.Create("Model"));
            ModelObject.GetComponent<Transform>().LocalPosition = Vector3f.Zero;
            ModelObject.GetComponent<Transform>().LocalRotation = Vector3f.Zero;
            ModelObject.GetComponent<Transform>().LocalScale = Vector3f.One;
        }

        public ModelPart Register(ModelPart part)
        {
            if (Parts.ContainsKey(part.Name))
                return null!;

            string name = part.Name;

            ModelObject.AddChild(GameObject.Create(name));

            GameObject partObject = ModelObject.GetChild(name);

            partObject.AddComponent(ShaderManager.Get("default"));
            partObject.AddComponent(Mesh.Create(name + "_Mesh_" + new Random().Next(), [], []));
            partObject.AddComponent(TextureAtlasManager.Get("entities").Atlas);
            partObject.AddComponent(TextureAtlasManager.Get("entities"));

            ModelPart addedPart = partObject.AddComponent(part);

            Parts.Add(part.Name, partObject);


            name = part.Name + "_damage_";

            ModelObject.AddChild(GameObject.Create(name));

            partObject = ModelObject.GetChild(name);

            partObject.AddComponent(ShaderManager.Get("default"));
            partObject.AddComponent(Mesh.Create(name + "_Mesh_Damage_" + new Random().Next(), [], []));
            partObject.AddComponent(TextureAtlasManager.Get("entities").Atlas);
            partObject.AddComponent(TextureAtlasManager.Get("entities"));
            partObject.Transform.LocalScale = Vector3f.One * 1.01f;

            partObject.Active = false;

            partObject.AddComponent(ModelPart.Create(name, "red"));

            Parts.Add(name, partObject);

            DamageMeshes.Add(name, partObject);

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

                if (DamageMeshes.TryGetValue(name, out GameObject? damageValue))
                {
                    GameObjectManager.Unregister(damageValue.Name);
                    DamageMeshes.Remove(name);
                }
            }
        }

        public void ActivateDamageMeshes()
        {
            foreach (var damageMesh in DamageMeshes.Values)
                damageMesh.Active = true;
        }

        public void DeactivateDamageMeshes()
        {
            foreach (var damageMesh in DamageMeshes.Values)
                damageMesh.Active = false;
        }
    }
}