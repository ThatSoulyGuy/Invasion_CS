using Invasion.Core;
using Invasion.ECS;
using Invasion.Render;
using System.Numerics;
using Vortice.Mathematics;

namespace Invasion.Entity.Entities
{
    public class EntityPlayer() : IEntity(100.0f, 6.0f, 12.0f)
    {
        public override string RegistryName => "entity_player";
        public float MouseSensitivity { get; set; } = 2.0f;
        public GameObject RenderCamera => GameObject.GetChild("Camera");

        public override void Initialize()
        {
            base.Initialize();

            InputManager.SetCursorMode(true);

            GameObject.AddChild(GameObject.Create("Camera"));

            RenderCamera.AddComponent(Camera.Create(45.0f, 0.01f, 1000.0f));
            RenderCamera.Transform.LocalPosition = new(0.0f, 0.0f, 3.0f);
        }

        public override void Update()
        {
            base.Update();

            UpdateMouselook();
            UpdateMovement();
        }

        private void UpdateMouselook()
        {
            Vector2 mouseMovement = InputManager.GetMouseMovementOffsets();

            mouseMovement *= MouseSensitivity;
            mouseMovement *= InputManager.DeltaTime;
            
            GameObject.Transform.Rotate(new(mouseMovement.Y, mouseMovement.X, 0.0f));

            GameObject.Transform.LocalRotation = new(
                MathHelper.Clamp(GameObject.Transform.LocalRotation.X, -89.0f, 89.0f),
                GameObject.Transform.LocalRotation.Y,
                GameObject.Transform.LocalRotation.Z
            );

            GameObject.Transform.LocalRotation = new(
                GameObject.Transform.LocalRotation.X,
                GameObject.Transform.LocalRotation.Y % 360.0f,
                GameObject.Transform.LocalRotation.Z
            );

            InputManager.ResetMouseDelta();
        }

        private void UpdateMovement()
        {
            var movement = Vector3.Zero;

            if (InputManager.GetKeyHeld(KeyCode.W))
                movement += GameObject.Transform.Forward;

            if (InputManager.GetKeyHeld(KeyCode.S))
                movement -= GameObject.Transform.Forward;

            if (InputManager.GetKeyHeld(KeyCode.A))
                movement -= GameObject.Transform.Right;

            if (InputManager.GetKeyHeld(KeyCode.D))
                movement += GameObject.Transform.Right;

            if (movement != Vector3.Zero)
            {
                movement = Vector3.Normalize(movement);
                movement *= InputManager.GetKeyHeld(KeyCode.LeftShift) ? RunningSpeed : WalkingSpeed;
            }

            GameObject.Transform.LocalPosition += movement * InputManager.DeltaTime;
        }
    }
}
