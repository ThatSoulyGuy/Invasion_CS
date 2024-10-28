using Invasion.Math;

namespace Invasion.Entity.Entities
{
    public class EntityPig() : AIEntity(10.0f, 4.5f, 6.0f)
    {
        public override string RegistryName => "entity_pig";

        public override Vector3f ColliderSpecification { get; } = new(0.5f, 0.5f, 0.5f);

        public Vector3f PlayerPosition { get; set; } = Vector3f.Zero;

        public override void Update()
        {
            base.Update();

            PathfindToPoint(PlayerPosition, true);

            GameObject.GetChild("Model").Transform.LocalPosition = new(0.0f, -0.487f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalRotation = new(0.0f, 180.0f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalScale = new(0.062f);
        }
    }
}
