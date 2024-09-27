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
        }

        public Vector3f PositionNoComponent { get; set; } = Vector3f.Zero;
        public Vector3f Size { get; set; } = Vector3f.One;

        private bool IsComponent => GameObject != null;
        private bool IsCleanedUp { get; set; } = false;

        private BoundingBox() { }

        ~BoundingBox()
        {
            if (!IsCleanedUp)
                CleanUp();
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

            BoundingBoxManager.Register(result);

            return result;
        } 
    }
}
