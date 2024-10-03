using Invasion.Entity.Model;

namespace Invasion.Entity.Models
{
    public class ModelPig : EntityModel
    {
        public override void Initialize()
        {
            base.Initialize();

            ModelPart body = Register(ModelPart.Create("body", "pig"));

            body.GameObject.Transform.PivotPoint = new(-5.0f, 7.0f, -5.0f);
            body.GameObject.Transform.LocalRotation = new(-90.0f, 0.0f, 0.0f);
            body.AddCube(new(-5.0f, 7.0f, -5.0f), new(10.0f, 16.0f, 8.0f));

            body.Generate();
        }
    }
}
