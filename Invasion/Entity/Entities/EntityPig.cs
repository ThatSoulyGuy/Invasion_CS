using Invasion.Core;
using Invasion.Math;

namespace Invasion.Entity.Entities
{
    public class EntityPig() : IEntity(10.0f, 4.5f, 6.0f)
    {
        public override string RegistryName => "entity_pig";

        public Vector3f PlayerPosition { get; set; } = Vector3f.Zero;

        public override void Update()
        {
            base.Update();

            //GameObject.Transform.Rotate(new Vector3f(0.0f, 0.01f, 0.0f));

            Vector3f lookAt = LookAt(GameObject.Transform.WorldPosition, PlayerPosition);

            GameObject.Transform.LocalRotation = new(0.0f, lookAt.Y, 0.0f);

            GameObject.Transform.Translate(GameObject.Transform.Forward * WalkingSpeed * InputManager.DeltaTime);

            GameObject.GetChild("Model").Transform.LocalPosition = new(0.0f, -0.487f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalRotation = new(0.0f, 180.0f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalScale = new(0.062f);
        }
    }
}
