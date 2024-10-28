using Invasion.ECS;
using Invasion.Math;
using Invasion.World;

namespace Invasion.Entity.Entities
{
    public class EntityLaserBeam : IEntity
    {
        public override string RegistryName => "entity_laser_beam";

        public override Vector3f ColliderSpecification => new(0.1f, 0.1f, 0.1f);

        private Vector3f Direction { get; set; }
        private float Speed { get; set; }

        private float LifeTime { get; set; } = 0.0f;
        private float LifeTimeStart { get; set; } = 30.0f;

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
        }

        public override void Update()
        {
            base.Update();

            LifeTime -= 0.05f;

            if (LifeTime <= 0.0f)
                InvasionMain.Overworld.GetComponent<IWorld>().KillEntity(this);

            //GameObject.Transform.LocalPosition += Direction * Speed;
        }

        public override void OnCollide(GameObject other)
        {
            base.OnCollide(other);

            if (other.HasComponent<EntityPlayer>())
                return;

            InvasionMain.Overworld.GetComponent<IWorld>().KillEntity(this);
        }
    }
}
