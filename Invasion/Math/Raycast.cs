using System;

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
        /// <returns>A tuple containing a boolean indicating if a hit occurred and the hit information.</returns>
        public static (bool, RayHitInformation) Cast(Vector3d origin, Vector3d normalizedDirection, double distance)
        {
            bool wasHit = false;

            RayHitInformation result = new()
            {
                Origin = origin,
                Direction = normalizedDirection,
                Distance = (float)distance
            };

            var boundingBoxes = BoundingBoxManager.GetAll();

            double closestDistance = double.MaxValue;
            RayHitInformation closestHitInfo = new();

            foreach (var box in boundingBoxes)
            {
                if (Intersect(origin, normalizedDirection, box, out double tmin, out double tmax, out Vector3d normal))
                {
                    if (tmin < 0)
                        tmin = tmax;

                    if (tmin >= 0 && tmin <= distance && tmin < closestDistance)
                    {
                        closestDistance = tmin;

                        Vector3d hitPoint = origin + normalizedDirection * tmin;

                        closestHitInfo = new RayHitInformation
                        {
                            Origin = origin,
                            Direction = normalizedDirection,
                            HitPoint = hitPoint,
                            Normal = normal,
                            Distance = (float)tmin,
                            Collider = box
                        };

                        wasHit = true;
                    }
                }
            }

            if (wasHit)
                return (true, closestHitInfo);
            else
                return (false, result);
        }

        /// <summary>
        /// Checks if a ray intersects with a bounding box.
        /// </summary>
        /// <param name="rayOrigin">The origin of the ray.</param>
        /// <param name="rayDirection">The direction vector of the ray.</param>
        /// <param name="box">The bounding box to check against.</param>
        /// <param name="tmin">The minimum distance along the ray where the intersection occurs.</param>
        /// <param name="tmax">The maximum distance along the ray where the intersection occurs.</param>
        /// <param name="normal">The normal vector at the point of intersection.</param>
        /// <returns>True if the ray intersects the bounding box; otherwise, false.</returns>
        private static bool Intersect(Vector3d rayOrigin, Vector3d rayDirection, BoundingBox box, out double tmin, out double tmax, out Vector3d normal)
        {
            tmin = 0;
            tmax = double.MaxValue;
            normal = Vector3d.Zero;

            double tminTemp = tmin;
            double tmaxTemp = tmax;
            Vector3d normalTemp = normal;

            if (!RaySlabIntersect(rayOrigin.X, rayDirection.X, box.Min.X, box.Max.X, ref tminTemp, ref tmaxTemp, ref normalTemp, new Vector3d(1, 0, 0)))
                return false;

            if (!RaySlabIntersect(rayOrigin.Y, rayDirection.Y, box.Min.Y, box.Max.Y, ref tminTemp, ref tmaxTemp, ref normalTemp, new Vector3d(0, 1, 0)))
                return false;

            if (!RaySlabIntersect(rayOrigin.Z, rayDirection.Z, box.Min.Z, box.Max.Z, ref tminTemp, ref tmaxTemp, ref normalTemp, new Vector3d(0, 0, 1)))
                return false;

            tmin = tminTemp;
            tmax = tmaxTemp;
            normal = normalTemp;
            return true;
        }

        /// <summary>
        /// Helper method to perform ray-slab intersection for a single axis.
        /// </summary>
        private static bool RaySlabIntersect(double rayOrigin, double rayDirection, double slabMin, double slabMax, ref double tmin, ref double tmax, ref Vector3d normal, Vector3d axisNormal)
        {
            const double epsilon = 1e-14;

            if (System.Math.Abs(rayDirection) < epsilon)
            {
                if (rayOrigin < slabMin || rayOrigin > slabMax)
                    return false;
            }
            else
            {
                double invD = 1.0 / rayDirection;
                double t0 = (slabMin - rayOrigin) * invD;
                double t1 = (slabMax - rayOrigin) * invD;

                Vector3d n = axisNormal;

                if (invD < 0.0)
                {
                    (t0, t1) = (t1, t0);
                    n = -axisNormal;
                }

                if (t0 > tmin)
                {
                    tmin = t0;
                    normal = n;
                }
                if (t1 < tmax)
                    tmax = t1;

                if (tmax <= tmin)
                    return false;
            }
            return true;
        }
    }
}
