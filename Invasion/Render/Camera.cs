using Invasion.ECS;
using System.Numerics;
using Vortice.Mathematics;

namespace Invasion.Render
{
    public class Camera : Component
    {
        public float FieldOfView { get; set; }
        public float NearPlane { get; set; }
        public float FarPlane { get; set; }

        public Matrix4x4 Projection => Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(MathHelper.ToRadians(FieldOfView), InvasionMain.Window!.ClientSize.Width / (float)InvasionMain.Window!.ClientSize.Height, NearPlane, FarPlane);
        public Matrix4x4 View => Matrix4x4.CreateLookAtLeftHanded(GameObject.Transform.WorldPosition, GameObject.Transform.WorldPosition + GameObject.Transform.Forward, Vector3.UnitY);

        private Camera() { }
         
        public static Camera Create(float fieldOfView, float nearPlane, float farPlane)
        {
            return new()
            {
                FieldOfView = fieldOfView,
                NearPlane = nearPlane,
                FarPlane = farPlane
            };
        }
    }
}
