using Invasion.Core;
using Invasion.ECS;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Invasion.Math
{
    public class Rigidbody : Component
    {
        public const float Gravity = -9.81f;

        public float Mass { get; set; } = 1.0f;
        public float Drag { get; set; } = 0.01f;

        public Vector3f Velocity = Vector3f.Zero;

        public bool IsGrounded { get; private set; } = false;

        private Rigidbody() { }

        private const float Epsilon = 0.0001f;
        private const float MaxVelocity = 50.0f;

        public override void Update()
        {
            Task.Run(() =>
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
                    Velocity.Y += Gravity * stepTime;
                    Velocity *= (1 - Drag);

                    ClampVelocity(MaxVelocity);

                    Vector3f displacement = Velocity * stepTime;

                    GameObject.Transform.Translate(displacement);

                    IsGrounded = false;

                    var colliders = BoundingBoxManager.GetAll().Where(x => x != collider).ToList();

                    foreach (var otherCollider in colliders)
                    {
                        if (collider.Intersects(otherCollider))
                        {
                            Vector3f penetrationVector = ComputePenetrationDepth(collider, otherCollider);

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
            });
        }

        public void AddForce(Vector3f force)
        {
            Velocity += force / Mass;
        }

        public void Move(Vector3f inputDirection, float speed)
        {
            Vector3f direction = inputDirection.Length() > 0 ? Vector3f.Normalize(inputDirection) : Vector3f.Zero;

            Vector3f desiredVelocity = direction * speed;

            Velocity.X = desiredVelocity.X;
            Velocity.Z = desiredVelocity.Z;

            if (IsGrounded && Velocity.Y < 0)
                Velocity.Y = 0;
        }

        private void ClampVelocity(float maxSpeed)
        {
            if (Velocity.Length() > maxSpeed)
                Velocity = Vector3f.Normalize(Velocity) * maxSpeed;
        }

        private Vector3f ComputePenetrationDepth(BoundingBox a, BoundingBox b)
        {
            Vector3f aHalfSize = (a.Max - a.Min) * 0.5f;
            Vector3f bHalfSize = (b.Max - b.Min) * 0.5f;

            Vector3f aCenter = a.Min + aHalfSize;
            Vector3f bCenter = b.Min + bHalfSize;

            Vector3f delta = bCenter - aCenter;
            Vector3f overlap = new Vector3f(
                aHalfSize.X + bHalfSize.X - MathF.Abs(delta.X),
                aHalfSize.Y + bHalfSize.Y - MathF.Abs(delta.Y),
                aHalfSize.Z + bHalfSize.Z - MathF.Abs(delta.Z));

            if (overlap.X < -Epsilon || overlap.Y < -Epsilon || overlap.Z < -Epsilon)
                return Vector3f.Zero;

            if (overlap.X < overlap.Y && overlap.X < overlap.Z)
                return new Vector3f(delta.X > 0 ? -overlap.X : overlap.X, 0, 0);
            else if (overlap.Y < overlap.X && overlap.Y < overlap.Z)
                return new Vector3f(0, delta.Y > 0 ? -overlap.Y : overlap.Y, 0);
            else
                return new Vector3f(0, 0, delta.Z > 0 ? -overlap.Z : overlap.Z);
        }

        public static Rigidbody Create()
        {
            return new();
        }
    }
}
 