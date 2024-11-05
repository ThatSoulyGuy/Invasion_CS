using Invasion.ECS;
using Invasion.Math;
using Invasion.World;
using System;

namespace Invasion.Entity.Entities
{
    public class EntityLaserBeam : IEntity
    {
        public override string RegistryName => "entity_laser_beam";

        public override Vector3f ColliderSpecification => new(0.1f, 0.1f, 0.1f);

        private Vector3f Direction { get; set; }
        private float Speed { get; set; }

        private float LifeTime { get; set; } = 0.0f;
        private float LifeTimeStart { get; set; } = 10.0f;

        public EntityLaserBeam(Vector3f direction, float speed) : base(0.0f, 0.0f, 0.0f) 
        {
            Direction = direction;
            Speed = speed;

            LifeTime = LifeTimeStart;
        }

        public override void Initialize()
        {
            base.Initialize();
            
            GameObject.GetComponent<Rigidbody>().UseGravity = false;

            GameObject.GetChild("Model").Transform.LocalRotation = new(0.0f, 180.0f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalScale = new(0.062f);
        }

        public override void Update()
        {
            base.Update();

            LifeTime -= 0.05f;

            if (LifeTime <= 0.0f)
                InvasionMain.Overworld.GetComponent<IWorld>().KillEntity(this);

            if (GameObject.Transform == null)
                return;

            GameObject.Transform.LocalPosition += Direction * Speed;
        }

        public override void OnCollide(GameObject other)
        {
            if (other == null)
                return;

            lock (other)
            {
                base.OnCollide(other);

                if (other.HasComponent<EntityPlayer>())
                    return;

                InvasionMain.Overworld.GetComponent<IWorld>().KillEntity(this);
            }
        }
    }
}
