using Invasion.ECS;
using Invasion.Math;
using Invasion.World.SpawnManagers;
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

        public abstract Vector3f ColliderSpecification { get; }

        public float Health { get; private set; } = maxHealth;
        public float MaxHealth { get; set; } = maxHealth;
        public float WalkingSpeed { get; set; } = walkingSpeed;
        public float RunningSpeed { get; set; } = runningSpeed;

        public bool IsDead => Health <= 0;

        public override void Update()
        {
            if (GameObject.Transform != null && GameObject.Transform.WorldPosition.Y < -20)
                Health = 0;

            if (IsDead)
                OnDeath();
        }

        public void Damage(float amount)
        {
            Health -= amount;

            OnDamaged(amount);
        }

        public void SetHealth(float amount)
        {
            float previousHealth = Health;

            Health = amount;

            if (Health < previousHealth)
                OnDamaged(previousHealth - Health);
        }

        public virtual void OnDamaged(float amount) { }

        public virtual void OnDeath() { }

        public virtual void Download(IEntityBehavior behavior)
        {
            behavior.Apply(this);
        }
    }
}