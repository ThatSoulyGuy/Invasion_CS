using Invasion.Entity.Model;
using Invasion.Math;

namespace Invasion.Entity.Models
{
    public class ModelPig : EntityModel
    {
        public override void Initialize()
        {
            base.Initialize();

            ModelPart body = Register(ModelPart.Create("body", "pig", this));

            body.GameObject.Transform.PivotPoint = new Vector3f(0.0f, 13.0f, 1.0f);
            body.GameObject.Transform.LocalRotation = new Vector3f(-90.0f, 0.0f, 0.0f);

            Vector2f[] bodyUVs = GetUVs(new Vector2f(28, 8), 10, 16, 8);
            body.AddCube(new Vector3f(-5.0f, 7.0f, -5.0f), new Vector3f(10.0f, 16.0f, 8.0f), bodyUVs);
            body.Generate();

            ModelPart head = Register(ModelPart.Create("head", "pig", this));
            Vector2f[] headUVs = GetUVs(new Vector2f(0, 0), 8, 8, 8);
            head.AddCube(new Vector3f(-4.0f, 8.0f, -14.0f), new Vector3f(8.0f, 8.0f, 8.0f), headUVs);

            Vector2f[] snoutUVs = GetUVs(new Vector2f(16, 16), 4, 3, 1);
            head.AddCube(new Vector3f(-2.0f, 9.0f, -15.0f), new Vector3f(4.0f, 3.0f, 1.0f), snoutUVs);
            head.Generate();

            ModelPart leftFrontLeg = Register(ModelPart.Create("left_front_leg", "pig", this));
            Vector2f[] legUVs = GetUVs(new Vector2f(0, 16), 4, 6, 4);
            leftFrontLeg.AddCube(new Vector3f(-5.0f, 0.0f, -7.0f), new Vector3f(4.0f, 6.0f, 4.0f), legUVs);
            leftFrontLeg.Generate();

            ModelPart rightFrontLeg = Register(ModelPart.Create("right_front_leg", "pig", this));
            rightFrontLeg.AddCube(new Vector3f(1.0f, 0.0f, -7.0f), new Vector3f(4.0f, 6.0f, 4.0f), legUVs);
            rightFrontLeg.Generate();

            ModelPart leftBackLeg = Register(ModelPart.Create("left_back_leg", "pig", this));
            leftBackLeg.AddCube(new Vector3f(-5.0f, 0.0f, 5.0f), new Vector3f(4.0f, 6.0f, 4.0f), legUVs);
            leftBackLeg.Generate();

            ModelPart rightBackLeg = Register(ModelPart.Create("right_back_leg", "pig", this));
            rightBackLeg.AddCube(new Vector3f(1.0f, 0.0f, 5.0f), new Vector3f(4.0f, 6.0f, 4.0f), legUVs);
            rightBackLeg.Generate();
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
