using Invasion.ECS;

namespace Invasion.Math
{
    public class BoundingBox : Component
    {
        public Vector3f Min
        {
            get
            {
                if (IsComponent)
                    return GameObject.Transform.WorldPosition - (Size / 2);
                else
                    return PositionNoComponent - (Size / 2);
            }
        }

        public Vector3f Max
        {
            get
            {
                if (IsComponent)
                    return GameObject.Transform.WorldPosition + (Size / 2);
                else
                    return PositionNoComponent + (Size / 2);
            }
        }

        public Vector3f Position
        {
            get => IsComponent ? GameObject.Transform.WorldPosition : PositionNoComponent;
            set
            {
                if (IsComponent)
                    GameObject.Transform.LocalPosition = value;
                else
                    PositionNoComponent = value;
            }
        }

        public Vector3f PositionNoComponent { get; set; } = Vector3f.Zero;
        public Vector3f Size { get; set; } = Vector3f.One;
        
        public bool IsComponent => GameObject != null;

        private const float Epsilon = 1e-6f;
        private bool IsCleanedUp { get; set; } = false;

        private BoundingBox() { }

        public bool Intersects(BoundingBox other)
        {
            return Min.X <= other.Max.X + Epsilon && Max.X >= other.Min.X - Epsilon &&
                   Min.Y <= other.Max.Y + Epsilon && Max.Y >= other.Min.Y - Epsilon &&
                   Min.Z <= other.Max.Z + Epsilon && Max.Z >= other.Min.Z - Epsilon;
        }

        public BoundingBox GetSweptBroadphaseBox(Vector3f displacement)
        {
            BoundingBox broadphaseBox = new()
            {
                Position = displacement.X > 0 ? Min : Min + displacement,
                Size = displacement.Abs()
            };

            return broadphaseBox;
        }

        public override void CleanUp()
        {
            IsCleanedUp = true;
            BoundingBoxManager.Unregister(this);
        }

        public static BoundingBox Create(Vector3f size)
        {
            return Create(Vector3f.Zero, size);
        }

        public static BoundingBox Create(Vector3f position, Vector3f size)
        {
            BoundingBox result = new()
            {
                PositionNoComponent = position,
                Size = size
            };

            return result;
        }
    }
}
