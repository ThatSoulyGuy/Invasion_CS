using Invasion.ECS;
using Invasion.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Invasion.Entity
{
    public abstract class AIEntity(float maxHealth, float walkingSpeed, float runningSpeed) : IEntity(maxHealth, walkingSpeed, runningSpeed)
    {
        private Queue<Vector3f> PathNodes { get; } = [];

        private const float NodeThreshold = 0.5f;
        private const float MaxNodeDistance = 100.0f;

        private BoundingBox SelfBoundingBox => GameObject.GetComponent<BoundingBox>();

        protected void PathfindToPoint(Vector3f targetPoint, bool useRigidbody)
        {
            List<BoundingBox> boundingBoxes = BoundingBoxManager.GetAll();
            Vector3f currentPosition = GameObject.Transform.WorldPosition;

            if (!IsPathPossible(currentPosition, targetPoint, boundingBoxes))
                return;
            
            if (PathNodes.Count == 0)
            {
                List<Vector3f> calculatedPath = CalculatePath(currentPosition, targetPoint, boundingBoxes);

                if (calculatedPath.Count == 0)
                    return;

                foreach (Vector3f node in calculatedPath)
                    PathNodes.Enqueue(node);
            }

            FollowPathToNode(useRigidbody);
        }

        private bool IsPathPossible(Vector3f startPoint, Vector3f endPoint, List<BoundingBox> boundingBoxes)
        {
            BoundingBox startBox = SelfBoundingBox;
            BoundingBox targetBox = BoundingBox.Create(endPoint, startBox.Size);

            foreach (BoundingBox box in boundingBoxes)
            {
                if (startBox.Intersects(box) || targetBox.Intersects(box))
                    return false;
            }

            return true;
        }

        private List<Vector3f> CalculatePath(Vector3f startPoint, Vector3f endPoint, List<BoundingBox> boundingBoxes)
        {
            List<Vector3f> path = [];

            Vector3f currentPosition = startPoint;

            while (currentPosition.Distance(endPoint) > NodeThreshold && path.Count < MaxNodeDistance)
            {
                Vector3f nextStep = Vector3f.Normalize(endPoint - currentPosition) * NodeThreshold;

                BoundingBox nextBox = BoundingBox.Create(currentPosition + nextStep, SelfBoundingBox.Size);

                if (boundingBoxes.All(b => !nextBox.Intersects(b)))
                {
                    path.Add(currentPosition + nextStep);
                    currentPosition += nextStep;
                }
                else
                    break;
            }

            if (currentPosition.Distance(endPoint) <= NodeThreshold)
                path.Add(endPoint);
            
            return path;
        }

        private void FollowPathToNode(bool useRigidbody)
        {
            if (PathNodes.Count == 0)
                return;

            Vector3f currentNode = PathNodes.Peek();
            Vector3f currentPosition = GameObject.Transform.WorldPosition;

            if (currentPosition.Distance(currentNode) <= NodeThreshold)
                PathNodes.Dequeue();
            else
            {
                Vector3f direction = Vector3f.Normalize(currentNode - currentPosition);

                GameObject.Transform.LocalRotation = LookAt(currentPosition, currentNode);

                if (useRigidbody)
                    GameObject.GetComponent<Rigidbody>().Move(direction * 0.1f, 1.0f);
                else
                    GameObject.Transform.Translate(direction * 0.1f); 
            }
        }

        public static Vector3f LookAt(Vector3f sourcePosition, Vector3f targetPosition)
        {
            Vector3f direction = targetPosition - sourcePosition;
            direction = Vector3f.Normalize(direction);

            float yaw = MathF.Atan2(direction.X, direction.Z);

            float pitch = MathF.Atan2(direction.Y, MathF.Sqrt(direction.X * direction.X + direction.Z * direction.Z));

            float pitchDegrees = Vortice.Mathematics.MathHelper.ToDegrees(pitch);
            float yawDegrees = Vortice.Mathematics.MathHelper.ToDegrees(yaw);

            return new Vector3f(pitchDegrees, yawDegrees, 0);
        }
    }
}
