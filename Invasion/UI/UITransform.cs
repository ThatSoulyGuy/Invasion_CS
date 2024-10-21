using Invasion.Math;
using System.Numerics;

namespace Invasion.UI
{
    public class UITransform
    {
        public Vector2f LocalPosition { get; set; } = Vector2f.Zero;
        public Vector2f LocalScale { get; set; } = Vector2f.One;
        public float LocalRotation { get; set; } = 0.0f;

        public Vector2f WorldPosition
        {
            get
            {
                Matrix4x4 modelMatrix = GetModelMatrix();

                return new Vector2f(modelMatrix.M41, modelMatrix.M42);
            }
        }

        public float WorldRotation
        {
            get
            {
                Matrix4x4 modelMatrix = GetModelMatrix();

                return (float)System.Math.Atan2(modelMatrix.M21, modelMatrix.M11);
            }
        }

        public Vector2f WorldScale
        {
            get
            {
                Matrix4x4 modelMatrix = GetModelMatrix();

                return new Vector2f(modelMatrix.M11, modelMatrix.M22);
            }
        }

        public UITransform? Parent { get; set; }

        private UITransform() { }

        public void Translate(Vector2f translation)
        {
            LocalPosition += translation;
        }

        public void Rotate(float rotation)
        {
            LocalRotation += rotation;
        }

        public void ScaleBy(Vector2f scale)
        {
            LocalScale *= scale;
        }

        public void ScaleBy(float scale)
        {
            LocalScale *= scale;
        }

        public Vector2f Forward => new((float)System.Math.Cos(LocalRotation), (float)System.Math.Sin(LocalRotation));

        public Matrix4x4 GetModelMatrix()
        {
            Matrix4x4 translation = Matrix4x4.CreateTranslation(LocalPosition.X, LocalPosition.Y, 0.0f);
            Matrix4x4 rotation = Matrix4x4.CreateRotationZ(LocalRotation);
            Matrix4x4 scale = Matrix4x4.CreateScale(LocalScale.X, LocalScale.Y, 1.0f);

            Matrix4x4 localTransform = scale * rotation * translation;

            if (Parent != null)
                return localTransform * Parent.GetModelMatrix();

            return localTransform;
        }

        public static UITransform Create(Vector2f position, Vector2f scale, float rotation)
        {
            return new UITransform
            {
                LocalPosition = position,
                LocalScale = scale,
                LocalRotation = rotation
            };
        }
    }
}
