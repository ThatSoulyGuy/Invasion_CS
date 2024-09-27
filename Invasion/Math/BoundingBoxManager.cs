﻿using System.Collections.Generic;

namespace Invasion.Math
{
    public static class BoundingBoxManager
    {
        private static Dictionary<Vector3f, BoundingBox> Colliders { get; } = [];

        public static void Register(BoundingBox collider)
        {
            if (Colliders.ContainsKey(collider.Position))
                return;

            Colliders.Add(collider.Position, collider);
        }

        public static BoundingBox Get(Vector3f position)
        {
            return Colliders.TryGetValue(position, out BoundingBox? value) ? value : null!;
        }

        public static List<BoundingBox> GetAll()
        {
            return [.. Colliders.Values];
        }

        public static void Unregister(BoundingBox collider)
        {
            Colliders.Remove(collider.Position);
        }
    }
}
