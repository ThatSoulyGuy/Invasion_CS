using Invasion.ECS;
using System.Numerics;
using Vortice.Mathematics;

namespace Invasion.Math
{
    public class Transform : Component
    {
        public Vector3f LocalPosition { get; set; } = Vector3f.Zero;
        public Vector3f LocalRotation { get; set; } = Vector3f.Zero;
        public Vector3f LocalScale { get; set; } = Vector3f.One;

        public Vector3f PivotPoint { get; set; } = Vector3f.Zero;

        public Vector3f WorldPosition
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();
                return new(matrix.M41, matrix.M42, matrix.M43);
            }
        }

        public Vector3f WorldPositionTransposed
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix(true);
                return new(matrix.M41, matrix.M42, matrix.M43);
            }
        }

        public Vector3f WorldRotation
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();

                Quaternion rotation = Quaternion.CreateFromRotationMatrix(matrix);
                Vector3f eulerRotation = rotation.ToEuler();

                return eulerRotation;
            }
        }

        public Vector3f WorldScale
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();

                Vector3f scaleX = new(matrix.M11, matrix.M12, matrix.M13);
                Vector3f scaleY = new(matrix.M21, matrix.M22, matrix.M23);
                Vector3f scaleZ = new(matrix.M31, matrix.M32, matrix.M33);

                return new(scaleX.Length(), scaleY.Length(), scaleZ.Length());
            }
        }

        public Vector3f Forward => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitZ, GetModelMatrix()));
        public Vector3f Right => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitX, GetModelMatrix()));
        public Vector3f Up => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitY, GetModelMatrix()));

        public Vector3f ForwardTransposed => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitZ, GetModelMatrix(true)));
        public Vector3f RightTransposed => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitX, GetModelMatrix(true)));
        public Vector3f UpTransposed => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitY, GetModelMatrix(true)));

        public Transform? Parent { get; internal set; } = null;

        private Transform() { }

        public void Translate(Vector3f translation)
        {
            LocalPosition += translation;
        }

        public void Rotate(Vector3f rotation)
        {
            LocalRotation += rotation;
        }

        public void RotateAround(Vector3f pivot, Vector3f axis, float angle)
        {
            Vector3f direction = LocalPosition - pivot;
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromAxisAngle(axis, angle);

            direction = Vector3f.Transform(direction, rotationMatrix);
            LocalPosition = pivot + direction;

            Rotate(axis * angle);
        }

        public void Scale(Vector3f scale)
        {
            LocalScale += scale;
        }

        public Matrix4x4 GetModelMatrix(bool transposeLocalT = false)
        {
            Matrix4x4 scale = LocalScale != Vector3f.One ? Matrix4x4.CreateScale(LocalScale) : Matrix4x4.Identity;
            Matrix4x4 rotation = LocalRotation != Vector3f.Zero
                ? Matrix4x4.CreateFromYawPitchRoll(
                    MathHelper.ToRadians(LocalRotation.Y),
                    MathHelper.ToRadians(LocalRotation.X),
                    MathHelper.ToRadians(LocalRotation.Z))
                : Matrix4x4.Identity;
            Matrix4x4 translation = Matrix4x4.CreateTranslation(LocalPosition);

            Matrix4x4 localTransform;

            if (transposeLocalT)
                localTransform = translation * rotation * scale;
            else
                localTransform = scale * rotation * translation;

            if (PivotPoint != Vector3f.Zero)
            {
                Matrix4x4 pivotTranslation = Matrix4x4.CreateTranslation(PivotPoint);
                Matrix4x4 pivotInverseTranslation = Matrix4x4.CreateTranslation(-PivotPoint);
                localTransform = pivotInverseTranslation * localTransform * pivotTranslation;
            }

            if (Parent != null)
                return localTransform * Parent.GetModelMatrix(transposeLocalT);
            else
                return localTransform;
        }

        public static Transform Create(Vector3f position, Vector3f rotation, Vector3f scale, Vector3f pivotPoint)
        {
            return new()
            {
                LocalPosition = position,
                LocalRotation = rotation,
                LocalScale = scale,
                PivotPoint = pivotPoint
            };
        }

        public static Transform Create()
        {
            return new();
        }
    }
}