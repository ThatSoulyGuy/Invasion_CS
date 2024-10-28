using Invasion.Core;
using Invasion.ECS;

namespace Invasion.Util
{
    public class DeleteAfter(float time) : Component
    {
        public float Time { get; set; } = time;

        public override void Update()
        {
            Time -= InputManager.DeltaTime;

            if (Time <= 0)
                GameObjectManager.Unregister(GameObject.Name);
        }
    }
}
