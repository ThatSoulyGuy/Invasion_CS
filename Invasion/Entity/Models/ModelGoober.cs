using Invasion.Entity.Model;
using Invasion.Math;

namespace Invasion.Entity.Models
{
    public class ModelGoober : EntityModel
    {
        public override void Initialize()
        {
            base.Initialize();

            ModelPart head = Register(ModelPart.Create("head", "goober", this));

            head.GameObject.Transform.PivotPoint = new(0, 18, 0);

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

            head.AddCube(new(new(-4, 18, -5), new(9, 15, 9), uvs));
            head.Generate();

            ModelPart body = Register(ModelPart.Create("body", "goober", this));

            body.GameObject.Transform.PivotPoint = new(0, 4, 0);

            body.AddCube(new(new(-6, 4, -3), new(12, 14, 5), uvs));
            body.Generate();

            ModelPart rightArm = Register(ModelPart.Create("right_arm", "goober", this));

            rightArm.GameObject.Transform.PivotPoint = new(6, 16, -0.5f);

            rightArm.AddCube(new(new(6, 6, -2), new(3, 11, 3), uvs));
            rightArm.Generate();

            ModelPart leftArm = Register(ModelPart.Create("left_arm", "goober", this));

            leftArm.GameObject.Transform.PivotPoint = new(-7, 16, -0.5f);

            leftArm.AddCube(new(new(-9, 6, -2), new(3, 11, 3), uvs));
            leftArm.Generate();

            ModelPart rightLeg = Register(ModelPart.Create("right_leg", "goober", this));

            rightLeg.GameObject.Transform.PivotPoint = new(3.5f, 4, -0.5f);

            rightLeg.AddCube(new(new(2f, 0, -2), new(3, 4, 3), uvs));
            rightLeg.Generate();

            ModelPart leftLeg = Register(ModelPart.Create("left_leg", "goober", this));

            leftLeg.GameObject.Transform.PivotPoint = new(-3.5f, 4, -0.5f);

            leftLeg.AddCube(new(new(-5, 0, -2), new(3, 4, 3), uvs));
            leftLeg.Generate();
        }

        private Vector2f[] GetUVs(Vector2f textureOffset, float width, float height, float depth)
        {
            Vector2f[] uvs = new Vector2f[24];

            float texWidth = 64.0f;
            float texHeight = 32.0f;

            uvs[0] = new Vector2f(textureOffset.X / texWidth, textureOffset.Y / texHeight);
            uvs[1] = new Vector2f((textureOffset.X + width) / texWidth, textureOffset.Y / texHeight);
            uvs[2] = new Vector2f((textureOffset.X + width) / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[3] = new Vector2f(textureOffset.X / texWidth, (textureOffset.Y + height) / texHeight);

            uvs[4] = new Vector2f((textureOffset.X + width + depth) / texWidth, textureOffset.Y / texHeight);
            uvs[5] = new Vector2f((textureOffset.X + 2 * width + depth) / texWidth, textureOffset.Y / texHeight);
            uvs[6] = new Vector2f((textureOffset.X + 2 * width + depth) / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[7] = new Vector2f((textureOffset.X + width + depth) / texWidth, (textureOffset.Y + height) / texHeight);

            uvs[8] = new Vector2f((textureOffset.X + width) / texWidth, textureOffset.Y / texHeight);
            uvs[9] = new Vector2f((textureOffset.X + width + depth) / texWidth, textureOffset.Y / texHeight);
            uvs[10] = new Vector2f((textureOffset.X + width + depth) / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[11] = new Vector2f((textureOffset.X + width) / texWidth, (textureOffset.Y + height) / texHeight);

            uvs[12] = new Vector2f(textureOffset.X / texWidth, textureOffset.Y / texHeight);
            uvs[13] = new Vector2f((textureOffset.X + depth) / texWidth, textureOffset.Y / texHeight);
            uvs[14] = new Vector2f((textureOffset.X + depth) / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[15] = new Vector2f(textureOffset.X / texWidth, (textureOffset.Y + height) / texHeight);

            uvs[16] = new Vector2f(textureOffset.X / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[17] = new Vector2f((textureOffset.X + width) / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[18] = new Vector2f((textureOffset.X + width) / texWidth, (textureOffset.Y + height + depth) / texHeight);
            uvs[19] = new Vector2f(textureOffset.X / texWidth, (textureOffset.Y + height + depth) / texHeight);

            uvs[20] = new Vector2f((textureOffset.X + width + depth) / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[21] = new Vector2f((textureOffset.X + 2 * width + depth) / texWidth, (textureOffset.Y + height) / texHeight);
            uvs[22] = new Vector2f((textureOffset.X + 2 * width + depth) / texWidth, (textureOffset.Y + height + depth) / texHeight);
            uvs[23] = new Vector2f((textureOffset.X + width + depth) / texWidth, (textureOffset.Y + height + depth) / texHeight);

            return uvs;
        }
    }
}
