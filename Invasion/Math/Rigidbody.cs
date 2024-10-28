using Invasion.Core;
using Invasion.ECS;
using System;

namespace Invasion.Math
{
    public class Rigidbody : Component
    {
        public const float Gravity = -9.81f;

        public float Mass { get; set; } = 1.0f;
        public float Drag { get; set; } = 0.01f;

        public float Magnitude => Velocity.Length();

        public Vector3f Velocity = Vector3f.Zero;

        public bool UseGravity { get; set; } = true;
        public bool IsGrounded { get; private set; } = false;

        private const float Epsilon = 0.0001f;
        private const float MaxVelocity = 50.0f;

        public override void Update()
        {
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
                if (UseGravity)
                    Velocity.Y += Gravity * stepTime;

                ClampVelocity(MaxVelocity);

                Vector3f displacement = Velocity;
                displacement.Multiply(stepTime);

                GameObject.Transform.Translate(displacement);

                IsGrounded = false;

                var colliders = BoundingBoxManager.GetAll();
                foreach (var otherCollider in colliders)
                {
                    if (otherCollider == collider)
                        continue;

                    if (otherCollider.Position.DistanceSquared(collider.Position) > 4.0f)
                        continue;

                    if (collider.Intersects(otherCollider))
                    {
                        Vector3f penetrationVector = ComputePenetrationDepth(collider, otherCollider);

                        GameObject.OnCollide(otherCollider.GameObject);
                        GameObject.Transform.Translate(penetrationVector);

                        if (MathF.Abs(penetrationVector.Y) > Epsilon)
                        {
                            if (penetrationVector.Y > 0 && Velocity.Y < 0)
                            {
                                Velocity.Y = 0;
                                IsGrounded = true;
                            }
                            else if (penetrationVector.Y < 0 && Velocity.Y > 0)
                                Velocity.Y = 0;
                        }

                        if (MathF.Abs(penetrationVector.X) > Epsilon)
                            Velocity.X = 0;

                        if (MathF.Abs(penetrationVector.Z) > Epsilon)
                            Velocity.Z = 0;
                    }
                }
            }
        }

        public void AddForce(Vector3f force)
        {
            Velocity.AddScaled(force, 1 / Mass);
        }

        public void Move(Vector3f inputDirection, float speed)
        {
            Vector3f direction = inputDirection;

            if (direction.LengthSquared() > 0)
                direction.Normalize();
            else
                direction = Vector3f.Zero;

            Vector3f desiredVelocity = direction;
            desiredVelocity.Multiply(speed);

            Velocity.X = desiredVelocity.X;
            Velocity.Z = desiredVelocity.Z;

            if (IsGrounded && Velocity.Y < 0)
                Velocity.Y = 0;
        }

        private void ClampVelocity(float maxSpeed)
        {
            float speedSquared = Velocity.LengthSquared();

            if (speedSquared > maxSpeed * maxSpeed)
            {
                float scale = maxSpeed / MathF.Sqrt(speedSquared);
                Velocity.Multiply(scale);
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
 