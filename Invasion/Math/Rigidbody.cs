using Invasion.Core;
using Invasion.ECS;
using Microsoft.VisualBasic.Devices;
using System;

namespace Invasion.Math
{
    public class Rigidbody : Component
    {
        public const float Gravity = -9.81f;

        private float _mass = 1.0f;
        public float Mass
        {
            get
            {
                lock (UpdateLock) { return _mass; }
            }
            set
            {
                lock (UpdateLock) { _mass = value; }
            }
        }

        private float _drag = 0.01f;
        public float Drag
        {
            get
            {
                lock (UpdateLock) { return _drag; }
            }
            set
            {
                lock (UpdateLock) { _drag = value; }
            }
        }

        private Vector3f _velocity = Vector3f.Zero;
        public float Magnitude
        {
            get
            {
                lock (UpdateLock) { return _velocity.Length(); }
            }
        }

        public bool UseGravity { get; set; } = true;
        public bool IsGrounded { get; private set; } = false;

        private const float Epsilon = 0.0001f;
        private const float MaxVelocity = 50.0f;

        private readonly object UpdateLock = new();

        public override void Update()
        {
            if (!GameObject.Active)
                return;

            BoundingBox collider = GameObject.GetComponent<BoundingBox>();

            if (collider == null)
            {
                Console.WriteLine("No BoundingBox component found on the GameObject.");
                return;
            }

            float deltaTime = InputManager.DeltaTime;
            float maxTimeStep = 0.02f;
            int steps = (int)MathF.Ceiling(deltaTime / maxTimeStep);
            float stepTime = deltaTime / steps;

            for (int i = 0; i < steps; i++)
            {
                ApplyGravity(stepTime);
                ApplyDrag();

                ClampVelocity(MaxVelocity);

                Vector3f displacement = _velocity * stepTime;

                if (GameObject.Transform != null)
                    GameObject.Transform.Translate(displacement);

                IsGrounded = false;
                CheckCollisions(collider);
            }
        }

        public void AddForce(Vector3f force)
        {
            lock (UpdateLock)
            {
                _velocity.AddScaled(force, 1 / _mass);
            }
        }

        public void Move(Vector3f inputDirection, float speed)
        {
            lock (UpdateLock)
            {
                Vector3f direction = inputDirection.LengthSquared() > 0 ? Vector3f.Normalize(inputDirection) : Vector3f.Zero;
                Vector3f desiredVelocity = direction * speed;

                _velocity.X = desiredVelocity.X;
                _velocity.Z = desiredVelocity.Z;

                if (IsGrounded && _velocity.Y < 0)
                    _velocity.Y = 0;
            }
        }

        private void ApplyGravity(float stepTime)
        {
            if (UseGravity)
            {
                _velocity.Y += Gravity * stepTime;
            }
        }

        private void ApplyDrag()
        {
            _velocity.Multiply(1 - _drag);
        }

        private void ClampVelocity(float maxSpeed)
        {
            float speedSquared = _velocity.LengthSquared();

            if (speedSquared > maxSpeed * maxSpeed)
            {
                float scale = maxSpeed / MathF.Sqrt(speedSquared);
                _velocity.Multiply(scale);
            }
        }

        private void CheckCollisions(BoundingBox collider)
        {
            var colliders = BoundingBoxManager.GetAll();
            bool isLargeCollider = IsLargeCollider(collider);

            BoundingBox enclosingCuboid = null!;

            if (isLargeCollider)
                enclosingCuboid = CreateEnclosingCuboid(collider);
            
            foreach (var otherCollider in colliders)
            {
                if (otherCollider == collider)
                    continue;

                if (isLargeCollider)
                {
                    if (!IsWithinEnclosingCuboid(enclosingCuboid, otherCollider))
                        continue;
                }
                else
                {
                    if ((otherCollider.Position - collider.Position).LengthSquared() > 4.0f)
                        continue;
                }

                if (collider.Intersects(otherCollider))
                {
                    Vector3f penetrationVector = ComputePenetrationDepth(collider, otherCollider);
                    GameObject.OnCollide(otherCollider.GameObject);

                    GameObject.Transform?.TranslateSafe(penetrationVector);

                    ResolveCollisions(penetrationVector);
                }
            }
        }

        private bool IsLargeCollider(BoundingBox collider)
        {
            Vector3f size = collider.Max - collider.Min;
            return size.X > 2.0f || size.Y > 2.0f || size.Z > 2.0f;
        }

        private BoundingBox CreateEnclosingCuboid(BoundingBox collider)
        {
            float margin = 2.0f;

            Vector3f expandedSize = collider.Size + new Vector3f(2 * margin, 2 * margin, 2 * margin);
            return BoundingBox.Create(collider.Position, expandedSize, true);
        }

        private bool IsWithinEnclosingCuboid(BoundingBox enclosingCuboid, BoundingBox otherCollider)
        {
            return enclosingCuboid.Intersects(otherCollider);
        }

        private void ResolveCollisions(Vector3f penetrationVector)
        {
            lock (UpdateLock)
            {
                if (MathF.Abs(penetrationVector.Y) > Epsilon)
                {
                    _velocity.Y = 0;
                    IsGrounded = penetrationVector.Y > 0;
                }

                if (MathF.Abs(penetrationVector.X) > Epsilon)
                    _velocity.X = 0;

                if (MathF.Abs(penetrationVector.Z) > Epsilon)
                    _velocity.Z = 0;
            }
        }

        private Vector3f ComputePenetrationDepth(BoundingBox a, BoundingBox b)
        {
            Vector3f aHalfSize = a.Max;
            aHalfSize.Subtract(a.Min);
            aHalfSize.Multiply(0.5f);

            Vector3f bHalfSize = b.Max;
            bHalfSize.Subtract(b.Min);
            bHalfSize.Multiply(0.5f);

            Vector3f aCenter = a.Min;
            aCenter.Add(aHalfSize);

            Vector3f bCenter = b.Min;
            bCenter.Add(bHalfSize);

            Vector3f delta = bCenter;
            delta.Subtract(aCenter);

            Vector3f overlap = new Vector3f(
                aHalfSize.X + bHalfSize.X - MathF.Abs(delta.X),
                aHalfSize.Y + bHalfSize.Y - MathF.Abs(delta.Y),
                aHalfSize.Z + bHalfSize.Z - MathF.Abs(delta.Z));

            if (overlap.X < -Epsilon || overlap.Y < -Epsilon || overlap.Z < -Epsilon)
                return Vector3f.Zero;

            if (overlap.X < overlap.Y && overlap.X < overlap.Z)
            {
                float penetrationX = delta.X > 0 ? -overlap.X : overlap.X;
                return new Vector3f(penetrationX, 0, 0);
            }
            else if (overlap.Y < overlap.X && overlap.Y < overlap.Z)
            {
                float penetrationY = delta.Y > 0 ? -overlap.Y : overlap.Y;
                return new Vector3f(0, penetrationY, 0);
            }
            else
            {
                float penetrationZ = delta.Z > 0 ? -overlap.Z : overlap.Z;
                return new Vector3f(0, 0, penetrationZ);
            }
        }

        public static Rigidbody Create()
        {
            return new();
        }
    }
}
 