using Invasion.Core;
using Invasion.ECS;
using Invasion.World;

namespace Invasion.Util
{
    public class DeleteAfter(float time) : Component
    {
        public float TimeAlive { get; set; } = time;

        public override void Update()
        {
            TimeAlive -= Time.DeltaTime;

            if (TimeAlive <= 0)
                GameObjectManager.Unregister(GameObject.Name);
        }
    }
}
