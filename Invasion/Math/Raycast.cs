using System;
using System.Collections.Generic;

namespace Invasion.Math
{
    public struct RayHitInformation
    {
        public Vector3f Origin { get; set; }
        public Vector3f Direction { get; set; }
        public Vector3f HitPoint { get; set; }  
        public Vector3f Normal { get; set; }
        public float Distance { get; set; }

        public BoundingBox Collider { get; set; }
    }

    public static class Raycast
    {
        /// <summary>
        /// Casts a ray from the origin in the specified direction and checks for intersections with registered bounding boxes.
        /// </summary>
        /// <param name="origin">The starting point of the ray.</param>
        /// <param name="normalizedDirection">The normalized direction vector of the ray.</param>
        /// <param name="distance">The maximum distance to check for intersections.</param>
        /// <param name="ignore">An optional bounding box to ignore during the cast.</param>
        /// <returns>A tuple containing a boolean indicating if a hit occurred and the hit information.</returns>
        public static (bool, RayHitInformation) Cast(Vector3d origin, Vector3d normalizedDirection, double distance, BoundingBox ignore = null!)
        {
            bool wasHit = false;
            RayHitInformation closestHitInfo = new();
            double minDistance = distance;

            var boundingBoxes = BoundingBoxManager.GetAll();

            List<BoundingBox> boxesWhereSizeNot1 = boundingBoxes.FindAll(box => box.Size != Vector3f.One);

            foreach (var box in boundingBoxes)
            {
                if (box == ignore)
                    continue;

                if (RayIntersectsAABB(origin, normalizedDirection, box, out double tmin, out Vector3d normal))
                {
                    if (tmin >= 0 && tmin <= minDistance)
                    {
                        minDistance = tmin;
                        wasHit = true;
                        closestHitInfo = new RayHitInformation
                        {
                            Origin = origin,
                            Direction = normalizedDirection,
                            HitPoint = origin + normalizedDirection * tmin,
                            Normal = normal,
                            Distance = (float)tmin,
                            Collider = box
                        };
                    }
                }
            }

            return (wasHit, closestHitInfo);
        }

        /// <summary>
        /// Determines if a ray intersects an axis-aligned bounding box (AABB).
        /// </summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The normalized direction of the ray.</param>
        /// <param name="box">The bounding box to test against.</param>
        /// <param name="tmin">The distance to the closest intersection point.</param>
        /// <param name="normal">The normal at the intersection point.</param>
        /// <returns>True if the ray intersects the bounding box; otherwise, false.</returns>
        private static bool RayIntersectsAABB(Vector3d origin, Vector3d direction, BoundingBox box, out double tmin, out Vector3d normal)
        {
            tmin = 0.0;
            double tmax = double.MaxValue;
            normal = Vector3d.Zero;

            Vector3d boxMin = box.Min;
            Vector3d boxMax = box.Max;

            for (int i = 0; i < 3; i++)
            {
                if (System.Math.Abs(direction[i]) < 1e-8)
                {
                    if (origin[i] < boxMin[i] || origin[i] > boxMax[i])
                    {
                        tmin = 0;
                        normal = Vector3d.Zero;
                        return false;
                    }
                }
                else
                {
                    double invD = 1.0 / direction[i];
                    double t0 = (boxMin[i] - origin[i]) * invD;
                    double t1 = (boxMax[i] - origin[i]) * invD;

                    if (invD < 0.0)
                    {
                        double temp = t0;
                        t0 = t1;
                        t1 = temp;
                    }

                    if (t0 > tmin)
                    {
                        tmin = t0;
                        
                        normal = Vector3d.Zero;
                        normal[i] = direction[i] < 0 ? 1 : -1;
                    }

                    if (t1 < tmax)
                    {
                        tmax = t1;
                    }

                    if (tmax < tmin)
                    {
                        tmin = 0;
                        normal = Vector3d.Zero;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
