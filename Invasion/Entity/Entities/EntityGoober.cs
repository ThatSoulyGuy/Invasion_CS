using Invasion.Core;
using Invasion.Entity.Models;
using Invasion.Math;
using Invasion.World;

namespace Invasion.Entity.Entities
{
    public class EntityGoober : AIEntity
    {
        public override string RegistryName => "entity_goober";

        public override BoundingBox ColliderSpecification { get; } = BoundingBox.Create(new(0.5f, 0.5f, 0.5f));

        private ModelGoober Model => GameObject.GetComponent<ModelGoober>()!;

        public EntityGoober() : base(30.0f, 0.1f, 0.2f) { }

        public override void Initialize()
        {
            base.Initialize();

            GameObject.GetChild("Model").Transform.LocalPosition = new(0.0f, -0.287f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalRotation = new(0.0f, 180.0f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalScale = new(0.062f);
        }
        
        public override void Update()
        {
            base.Update();

            Rigidbody rigidbody = GameObject.GetComponent<Rigidbody>()!;

            if (Model != null)
                Model.GetPart("head").GameObject.Transform.LocalRotation = LookAt(Model.GetPart("head").GameObject.Transform.WorldPosition, InvasionMain.Player.Transform.WorldPosition);

            GameObject.Transform.LocalRotation = new(0.0f, Model!.GetPart("head").GameObject.Transform.LocalRotation.Y, 0.0f);

            if (Vector3f.Distance(GameObject.Transform.WorldPosition, InvasionMain.Player.Transform.WorldPosition) < 2)
                InvasionMain.Player.GetComponent<EntityPlayer>().Health -= 0.1f;

            if (rigidbody.IsGrounded && Vector3f.Distance(GameObject.Transform.WorldPosition, InvasionMain.Player.Transform.WorldPosition) > 1.5)
                rigidbody.Move(GameObject.Transform.Forward * InputManager.DeltaTime, 4.5f);
        }

        public override void OnDeath()
        {
            base.OnDeath();

            InvasionMain.Overworld.GetComponent<IWorld>().KillEntity(this);
        }
    }
}
