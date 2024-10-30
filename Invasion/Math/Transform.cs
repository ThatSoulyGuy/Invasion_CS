using Invasion.ECS;
using System.Collections.Generic;
using System.Numerics;
using Vortice.Mathematics;

namespace Invasion.Math
{
    public class Transform : Component
    {
        private Vector3f _localPosition = Vector3f.Zero;
        public Vector3f LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                MarkDirty();
            }
        }

        private Vector3f _localRotation = Vector3f.Zero;
        public Vector3f LocalRotation
        {
            get => _localRotation;
            set
            {
                _localRotation = value;
                MarkDirty();
            }
        }

        private Vector3f _localScale = Vector3f.One;
        public Vector3f LocalScale
        {
            get => _localScale;
            set
            {
                _localScale = value;
                MarkDirty();
            }
        }

        public Vector3f PivotPoint { get; set; } = Vector3f.Zero;

        public Vector3f WorldPosition
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();
                return new Vector3f(matrix.M41, matrix.M42, matrix.M43);
            }
        }

        public Vector3f WorldRotation
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();
                return ExtractEulerAngles(matrix);
            }
        }

        public Vector3f WorldScale
        {
            get
            {
                Matrix4x4 matrix = GetModelMatrix();

                float scaleX = new Vector3f(matrix.M11, matrix.M12, matrix.M13).Length();
                float scaleY = new Vector3f(matrix.M21, matrix.M22, matrix.M23).Length();
                float scaleZ = new Vector3f(matrix.M31, matrix.M32, matrix.M33).Length();

                return new Vector3f(scaleX, scaleY, scaleZ);
            }
        }

        public Vector3f Forward => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitZ, GetModelMatrix()));
        public Vector3f Right => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitX, GetModelMatrix()));
        public Vector3f Up => Vector3f.Normalize(Vector3f.TransformNormal(Vector3f.UnitY, GetModelMatrix()));

        private Transform? _parent;
        public Transform? Parent
        {
            get => _parent;
            internal set
            {
                if (_parent != value)
                {
                    _parent?._children.Remove(this);
                    _parent = value;
                    _parent?._children.Add(this);

                    MarkDirty();
                }
            }
        }

        private readonly List<Transform> _children = new List<Transform>();

        private readonly object _lock = new();
        private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
        private bool _isDirty = true;

        private Transform() { }

        private void MarkDirty()
        {
            if (_isDirty) 
                return;

            _isDirty = true;

            foreach (var child in _children)
                child.MarkDirty();
        }

        public void Translate(Vector3f translation)
        {
            LocalPosition += translation;
        }

        public void TranslateSafe(Vector3f translation)
        {
            lock (_lock)
            {
                LocalPosition += translation;
            }
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
            if (_isDirty)
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
                    _modelMatrix = localTransform * Parent.GetModelMatrix(transposeLocalT);
                else
                    _modelMatrix = localTransform;

                _isDirty = false;
            }

            return _modelMatrix;
        }

        private Vector3f ExtractEulerAngles(Matrix4x4 matrix)
        {
            float pitch, yaw, roll;

            if (System.Math.Abs(matrix.M23) < 0.99999f)
            {
                pitch = (float)System.Math.Asin(-matrix.M23);
                yaw = (float)System.Math.Atan2(matrix.M13, matrix.M33);
                roll = (float)System.Math.Atan2(matrix.M21, matrix.M22);
            }
            else
            {
                pitch = (float)(System.Math.PI / 2 * System.Math.Sign(-matrix.M23));
                yaw = 0;
                roll = (float)System.Math.Atan2(-matrix.M12, matrix.M11);
            }

            return new Vector3f(
                MathHelper.ToDegrees(pitch),
                MathHelper.ToDegrees(yaw),
                MathHelper.ToDegrees(roll));
        }

        public static Transform Create(Vector3f position, Vector3f rotation, Vector3f scale, Vector3f pivotPoint)
        {
            return new Transform
            {
                LocalPosition = position,
                LocalRotation = rotation,
                LocalScale = scale,
                PivotPoint = pivotPoint
            };
        }

        public static Transform Create()
        {
            return new Transform();
        }
    }

}