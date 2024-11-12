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
        public GameObject ModelObject { get; private set; } = null!;

        private Dictionary<string, GameObject> Parts { get; } = new();
        private Dictionary<string, GameObject> DamageMeshes { get; } = new();

        protected EntityModel() { }

        public override void Initialize()
        {
            ModelObject =  GameObject.AddChild(GameObject.Create("Model"));

            ModelObject.GetComponent<Transform>().LocalPosition = Vector3f.Zero;
            ModelObject.GetComponent<Transform>().LocalRotation = Vector3f.Zero;
            ModelObject.GetComponent<Transform>().LocalScale = Vector3f.One;
        }

        public override void Update()
        {
            lock (this)
            {
                foreach (var part in Parts)
                {
                    foreach (var damagePart in DamageMeshes)
                    {
                        if (part.Key == damagePart.Key)
                        {
                            if (damagePart.Value.Transform != null && part.Value.Transform != null)
                                damagePart.Value.Transform.LocalPosition = part.Value.Transform.LocalPosition;

                            if (damagePart.Value.Transform != null && part.Value.Transform != null)
                                damagePart.Value.Transform.LocalRotation = part.Value.Transform.LocalRotation;

                            if (damagePart.Value.Transform != null && part.Value.Transform != null)
                                damagePart.Value.Transform.PivotPoint = part.Value.Transform.PivotPoint;
                        }
                    }
                }
            }
        }

        public ModelPart Register(ModelPart part)
        {
            if (Parts.ContainsKey(part.Name))
                return null!;

            string name = part.Name;

            GameObject partObject = null!;

            if (ModelObject == null)
            {
                System.Threading.Thread.Sleep(2);
                
                ModelObject!.AddChild(GameObject.Create(name));

                partObject = ModelObject!.GetChild(name);

                partObject.AddComponent(ShaderManager.Get("default"));
                partObject.AddComponent(Mesh.Create(name + "_Mesh_" + new Random().Next(), [], []));
                partObject.AddComponent(TextureAtlasManager.Get("entities").Atlas);
                partObject.AddComponent(TextureAtlasManager.Get("entities"));
            }
            else
            {
                System.Threading.Thread.Sleep(2);

                ModelObject!.AddChild(GameObject.Create(name));

                partObject = ModelObject!.GetChild(name);

                partObject.AddComponent(ShaderManager.Get("default"));
                partObject.AddComponent(Mesh.Create(name + "_Mesh_" + new Random().Next(), [], []));
                partObject.AddComponent(TextureAtlasManager.Get("entities").Atlas);
                partObject.AddComponent(TextureAtlasManager.Get("entities"));
            }

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

                if (DamageMeshes.TryGetValue(name, out GameObject? damageValue))
                {
                    GameObjectManager.Unregister(damageValue.Name);
                    DamageMeshes.Remove(name);
                }
            }
        }

        public ModelPart AddDamageMesh(ModelPart part)
        {
            if (DamageMeshes.ContainsKey(part.Name))
                return null!;

            GameObject damageObject = null!;

            if (ModelObject == null)
            {
                System.Threading.Thread.Sleep(2);

                ModelObject!.AddChild(GameObject.Create(part.Name + "_Damage"));

                damageObject = ModelObject.GetChild(part.Name + "_Damage");

                damageObject.AddComponent(ShaderManager.Get("default"));
                damageObject.AddComponent(Mesh.Create(part.Name + "_Damage_Mesh_" + new Random().Next(), [], []));
                damageObject.AddComponent(TextureAtlasManager.Get("entities").Atlas);
                damageObject.AddComponent(TextureAtlasManager.Get("entities"));
                damageObject.Transform.LocalScale = Vector3f.One * 1.02f;

                damageObject.Active = false;
            }
            else
            {
                System.Threading.Thread.Sleep(2);

                ModelObject!.AddChild(GameObject.Create(part.Name + "_Damage"));

                damageObject = ModelObject.GetChild(part.Name + "_Damage");

                damageObject.AddComponent(ShaderManager.Get("default"));
                damageObject.AddComponent(Mesh.Create(part.Name + "_Damage_Mesh_" + new Random().Next(), [], []));
                damageObject.AddComponent(TextureAtlasManager.Get("entities").Atlas);
                damageObject.AddComponent(TextureAtlasManager.Get("entities"));
                damageObject.Transform.LocalScale = Vector3f.One * 1.02f;

                damageObject.Active = false;
            }

            ModelPart addedPart = damageObject.AddComponent(ModelPart.Create(part.Name + "_Damage", "red", this));

            addedPart.Cubes = part.Cubes;

            addedPart.Generate(false);

            DamageMeshes.Add(part.Name, damageObject);

            return addedPart;
        }

        public void ActivateDamageMeshes()
        {
            foreach (GameObject part in Parts.Values)
                part.Active = false;

            foreach (var damageMesh in DamageMeshes.Values)
                damageMesh.Active = true;
        }

        public void DeactivateDamageMeshes()
        {
            foreach (var damageMesh in DamageMeshes.Values)
                damageMesh.Active = false;

            foreach (GameObject part in Parts.Values)
                part.Active = true;
        }
    }
}