using Invasion.Math;
using System;
using Vortice.Mathematics;

namespace Invasion.Entity.Entities
{
    public class EntityPig() : IEntity(10.0f, 4.5f, 6.0f)
    {
        public override string RegistryName => "entity_pig";

        public Vector3f PlayerPosition { get; set; } = Vector3f.Zero;

        public override void Update()
        {
            base.Update();

            GameObject.Transform.Rotate(new Vector3f(0.0f, 0.01f, 0.0f));

            GameObject.GetChild("Model").Transform.LocalPosition = new(0.0f, -0.487f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalScale = new(0.062f);
        }

        public static Vector3f LookAt(Vector3f sourcePosition, Vector3f targetPosition)
        {
            Vector3f direction = targetPosition - sourcePosition;
            direction = Vector3f.Normalize(direction);

            float yaw = MathF.Atan2(direction.X, direction.Z);

            float pitch = MathF.Atan2(direction.Y, MathF.Sqrt(direction.X * direction.X + direction.Z * direction.Z));

            float pitchDegrees = MathHelper.ToDegrees(pitch);
            float yawDegrees = MathHelper.ToDegrees(yaw);

            return new Vector3f(pitchDegrees, yawDegrees, 0);
        }
    }
}
