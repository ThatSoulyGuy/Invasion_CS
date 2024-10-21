using Invasion.Core;
using Invasion.Entity.Models;
using Invasion.Math;

namespace Invasion.Entity.Entities
{
    public class EntityGoober : AIEntity
    {
        public override string RegistryName => "entity_goober";

        public override BoundingBox ColliderSpecification { get; } = BoundingBox.Create(new(0.5f, 0.5f, 0.5f));

        private ModelGoober Model => GameObject.GetComponent<ModelGoober>()!;

        public EntityGoober() : base(20.0f, 0.1f, 0.2f) { }

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
            rigidbody.Move(GameObject.Transform.Forward * InputManager.DeltaTime, 4.5f);
        }
    }
}
