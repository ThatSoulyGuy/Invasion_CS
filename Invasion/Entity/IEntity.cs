using Invasion.ECS;
using Invasion.Math;
using System.Collections.Generic;

namespace Invasion.Entity
{
    public interface IEntityBehavior
    {
        void Apply(IEntity entity);
    }

    public class CompositeBehavior : IEntityBehavior
    {
        private List<IEntityBehavior> Behaviors { get; } = [];

        public void Add(IEntityBehavior behavior)
        {
            Behaviors.Add(behavior);
        }

        public void Apply(IEntity entity)
        {
            foreach (var behavior in Behaviors)
                behavior.Apply(entity);
        }
    }

    public abstract class IEntity(float maxHealth, float walkingSpeed, float runningSpeed) : Component
    {
        public abstract string RegistryName { get; }

        public abstract BoundingBox ColliderSpecification { get; }

        public float Health { get; set; } = maxHealth;
        public float MaxHealth { get; set; } = maxHealth;
        public float WalkingSpeed { get; set; } = walkingSpeed;
        public float RunningSpeed { get; set; } = runningSpeed;

        public bool IsDead => Health <= 0;

        public override void Update()
        {
            if (IsDead)
                OnDeath();
        }

        public virtual void OnDeath() { }

        public virtual void Download(IEntityBehavior behavior)
        {
            behavior.Apply(this);
        }
    }
}