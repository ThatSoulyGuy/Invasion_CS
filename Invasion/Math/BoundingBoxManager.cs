using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Invasion.Math
{
    public static class BoundingBoxManager
    {
        private static ConcurrentDictionary<Vector3f, BoundingBox> Colliders { get; } = [];

        public static bool Register(BoundingBox collider)
        {
            if (Colliders.ContainsKey(collider.Position))
                return false;

            Colliders.TryAdd(collider.Position, collider);

            return true;
        }

        public static bool Contains(Vector3f position)
        {
            return Colliders.ContainsKey(position);
        }

        public static bool Contains(BoundingBox collider)
        {
            return Colliders.Contains(new(collider.Position, collider));
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
            Colliders.TryRemove(collider.Position, out _);
        }
    }
}
