using Invasion.Entity.Model;

namespace Invasion.Entity.Models
{
    public class ModelPig : EntityModel
    {
        public override void Initialize()
        {
            base.Initialize();

            ModelPart body = Register(ModelPart.Create("body", "pig"));

            body.AddCube(new(-5.0f, 6.0f, -8.0f), new(10.0f, 8.0f, 16.0f));

            body.Generate();

            ModelPart head = Register(ModelPart.Create("head", "pig"));

            head.AddCube(new(-4.0f, 8.0f, -14.0f), new(8.0f, 8.0f, 8.0f));
            head.AddCube(new(-2.0f, 9.0f, -15.0f), new(4.0f, 3.0f, 1.0f));

            head.Generate();

            ModelPart leftFrontLeg = Register(ModelPart.Create("left_front_leg", "pig"));

            leftFrontLeg.AddCube(new(-5.0f, 0.0f, -7.0f), new(4.0f, 6.0f, 4.0f));

            leftFrontLeg.Generate();

            ModelPart rightFrontLeg = Register(ModelPart.Create("right_front_leg", "pig"));

            rightFrontLeg.AddCube(new(1.0f, 0.0f, -7.0f), new(4.0f, 6.0f, 4.0f));

            rightFrontLeg.Generate();

            ModelPart leftBackLeg = Register(ModelPart.Create("left_back_leg", "pig"));

            leftBackLeg.AddCube(new(-5.0f, 0.0f, 5.0f), new(4.0f, 6.0f, 4.0f));

            leftBackLeg.Generate();

            ModelPart rightBackLeg = Register(ModelPart.Create("right_back_leg", "pig"));

            rightBackLeg.AddCube(new(1.0f, 0.0f, 5.0f), new(4.0f, 6.0f, 4.0f));

            rightBackLeg.Generate();
        }
    }
}
