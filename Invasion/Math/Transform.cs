using Invasion.ECS;
using System.Numerics;
using Vortice.Mathematics;

namespace Invasion.Math
{
    public class Transform : Component
    {
        public Vector3 LocalPosition { get; set; } = Vector3.Zero;
        public Vector3 LocalRotation { get; set; } = Vector3.Zero;
        public Vector3 LocalScale { get; set; } = Vector3.One;

        public Vector3 WorldPosition
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();
                return new(matrix.M41, matrix.M42, matrix.M43);
            }
        }

        public Vector3 WorldRotation
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();

                Quaternion rotation = Quaternion.CreateFromRotationMatrix(matrix);
                Vector3 eulerRotation = rotation.ToEuler();

                return eulerRotation;
            }
        }

        public Vector3 WorldScale
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();

                Vector3 scaleX = new(matrix.M11, matrix.M12, matrix.M13);
                Vector3 scaleY = new(matrix.M21, matrix.M22, matrix.M23);
                Vector3 scaleZ = new(matrix.M31, matrix.M32, matrix.M33);

                return new Vector3(scaleX.Length(), scaleY.Length(), scaleZ.Length());
            }
        }

        public Vector3 Forward => Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, GetModelMatrix()));
        public Vector3 Right => Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitX, GetModelMatrix()));
        public Vector3 Up => Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, GetModelMatrix()));

        public Transform? Parent { get; internal set; } = null;

        private Transform() { }

        public void Translate(Vector3 translation)
        {
            LocalPosition += translation;
        }

        public void Rotate(Vector3 rotation)
        {
            LocalRotation += rotation;
        }

        public void Rotate(Vector3 axis, float angle)
        {
            LocalRotation += axis * angle;
        }

        public void Scale(Vector3 scale)
        {
            LocalScale += scale;
        }

        public Matrix4x4 GetModelMatrix()
        {
            Matrix4x4 translation = Matrix4x4.CreateTranslation(LocalPosition);
            Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(LocalRotation.Y, LocalRotation.X, LocalRotation.Z);
            Matrix4x4 scale = Matrix4x4.CreateScale(LocalScale);

            Matrix4x4 localTransform = scale * rotation * translation;

            if (Parent != null)
                return localTransform * Parent.GetModelMatrix();
            else
                return localTransform;
        }

        public static Transform Create(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            return new()
            {
                LocalPosition = position,
                LocalRotation = rotation,
                LocalScale = scale
            };
        }

        public static Transform Create()
        {
            return new();
        }
    }
}