using Invasion.Render;

namespace Invasion.ECS
{
    public abstract class Component
    {
        public GameObject GameObject { get; internal set; } = null!;

        public virtual void Initialize() { }

        public virtual void Update() { }
        public virtual void Render(Camera camera) { }

        public virtual void CleanUp() { }

        public virtual void OnCollide(GameObject other) { }
    }
}
