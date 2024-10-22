using Invasion.Entity.Model;
using Invasion.Math;

namespace Invasion.Entity.Models
{
    public class ModelLaserBeam : EntityModel
    {
        public override void Initialize()
        {
            base.Initialize();

            Vector2f[] uvs =
            [
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0),

                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0),

                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0),

                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0),

                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0),

                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0),
            ];
            
            ModelPart head = Register(ModelPart.Create("head", "laser_beam"));

            head.GameObject.Transform.PivotPoint = new(0, 0, 0);

            head.AddCube(new(new(7, 0, 3), new(2, 2, 10), uvs));
            head.Generate();
        }
    }
}