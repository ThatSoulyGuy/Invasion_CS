using Invasion.ECS;
using Invasion.Render;
using SharpGen.Runtime;
using System.Collections.Generic;

namespace Invasion.Math
{
    public class BoundingBox : Component
    {
        public Vector3f Min => Position - (Size / 2);
        public Vector3f Max => Position + (Size / 2);

        public Vector3f LocalMin => -(Size / 2);
        public Vector3f LocalMax => Size / 2;

        public Vector3f Position
        {
            get
            {
                if (GameObject != null && GameObject.Transform == null)
                    return PositionNoComponent;

                return IsComponent ? GameObject!.Transform.WorldPosition : PositionNoComponent;
            }
            set
            {
                if (GameObject != null && GameObject.Transform == null)
                {
                    PositionNoComponent = value;
                    return;
                }

                if (IsComponent)
                    GameObject!.Transform.LocalPosition = value;
                else
                    PositionNoComponent = value;
            }
        }

        public Vector3f PositionNoComponent { get; set; } = Vector3f.Zero;
        public Vector3f Size { get; set; } = Vector3f.One;
        
        public bool IsComponent => GameObject != null;

        private const float Epsilon = 1e-6f;
        private bool IsCleanedUp { get; set; } = false;
        private bool Registered { get; set; } = false;

        private BoundingBox() { }

        public override void Initialize()
        {
            if (!Registered)
                BoundingBoxManager.Register(this);

            Registered = true;

            GameObject.AddComponent(ShaderManager.Get("line"));

#if DEBUG
            List<Vertex> vertices =
            [
                new Vertex(new Vector3f(LocalMin.X, LocalMin.Y, LocalMin.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero), // 0
                new Vertex(new Vector3f(LocalMax.X, LocalMin.Y, LocalMin.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero), // 1
                new Vertex(new Vector3f(LocalMax.X, LocalMin.Y, LocalMax.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero), // 2
                new Vertex(new Vector3f(LocalMin.X, LocalMin.Y, LocalMax.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero), // 3
                new Vertex(new Vector3f(LocalMin.X, LocalMax.Y, LocalMin.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero), // 4
                new Vertex(new Vector3f(LocalMax.X, LocalMax.Y, LocalMin.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero), // 5
                new Vertex(new Vector3f(LocalMax.X, LocalMax.Y, LocalMax.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero), // 6
                new Vertex(new Vector3f(LocalMin.X, LocalMax.Y, LocalMax.Z), new Vector3f(1.0f, 1.0f, 1.0f), Vector3f.Zero, Vector2f.Zero)  // 7
            ];

            List<uint> indices =
            [
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                4, 5,
                5, 6,
                6, 7,
                7, 4,

                0, 4,
                1, 5,
                2, 6, 
                3, 7
            ];

            GameObject.AddComponent(LineMesh.Create("line_bb", vertices, indices));

            GameObject.GetComponent<LineMesh>().Generate();
#endif
        }

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

        public override string ToString()
        {
            return $"Position: {Position}, Size: {Size}";
        }

        public override void Update()
        {
            if (!BoundingBoxManager.Contains(this) && GameObject?.Transform != null)
                BoundingBoxManager.Register(this);
        }

        public override void CleanUp()
        {
            IsCleanedUp = true;
            BoundingBoxManager.Unregister(this);
        }

        public static BoundingBox Create(Vector3f size, bool doNotRegister = false)
        {
            return Create(Vector3f.Zero, size, doNotRegister);
        }

        public static BoundingBox Create(Vector3f position, Vector3f size, bool doNotRegister = false)
        {
            BoundingBox result = new()
            {
                PositionNoComponent = position,
                Size = size
            };

            result.Registered = BoundingBoxManager.Register(result);

            return result;
        }
    }
}
