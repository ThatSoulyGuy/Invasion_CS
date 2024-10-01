using Invasion.Block;
using Invasion.Core;
using Invasion.ECS;
using Invasion.Math;
using Invasion.Render;
using Invasion.World;
using System;
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
            RenderCamera.Transform.LocalPosition = new(0.0f, 0.9f, 3.0f);
        }

        public override void Update()
        {
            base.Update();

            UpdateControls();
            UpdateMouselook();
            UpdateMovement();
        }

        private void UpdateControls()
        {
            if (InputManager.MouseLeftPressed)
            {
                var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, Vector3f.Normalize(RenderCamera.Transform.Forward), 10.0f);

                Vector3f position = information.HitPoint;

                position += information.Normal * 0.5f;

                if (hit)
                    InvasionMain.Overworld.GetComponent<IWorld>().SetBlock(position, BlockList.AIR);
            }

            if (InputManager.MouseRightPressed)
            {
                var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, Vector3f.Normalize(RenderCamera.Transform.Forward), 10.0f);

                Vector3f position = information.HitPoint;

                position -= information.Normal * 0.5f;

                if (hit)
                    InvasionMain.Overworld.GetComponent<IWorld>().SetBlock(position, BlockList.BEDROCK, true);
            }
        }

        private void UpdateMouselook()
        {
            Vector2f mouseMovement = InputManager.GetMouseMovementOffsets();

            mouseMovement *= MouseSensitivity;
            mouseMovement *= InputManager.DeltaTime;

            RenderCamera.Transform.Rotate(new(mouseMovement.Y, mouseMovement.X, 0.0f));

            RenderCamera.Transform.LocalRotation = new(
                MathHelper.Clamp(RenderCamera.Transform.LocalRotation.X, -89.0f, 89.0f),
                RenderCamera.Transform.LocalRotation.Y,
                RenderCamera.Transform.LocalRotation.Z
            );

            RenderCamera.Transform.LocalRotation = new(
                RenderCamera.Transform.LocalRotation.X,
                RenderCamera.Transform.LocalRotation.Y % 360.0f,
                RenderCamera.Transform.LocalRotation.Z
            );

            InputManager.ResetMouseDelta();
        }

        private void UpdateMovement()
        {
            var movement = Vector3f.Zero;

            Rigidbody rigidbody = GameObject.GetComponent<Rigidbody>();

            Vector3f forward = RenderCamera.Transform.Forward;
            Vector3f right = RenderCamera.Transform.Right;
            forward.Y = 0.0f;
            right.Y = 0.0f;

            if (InputManager.GetKeyHeld(KeyCode.W))
                movement += forward;

            if (InputManager.GetKeyHeld(KeyCode.S))
                movement -= forward;

            if (InputManager.GetKeyHeld(KeyCode.A))
                movement -= right;

            if (InputManager.GetKeyHeld(KeyCode.D))
                movement += right;

            if (InputManager.GetKeyHeld(KeyCode.Space) && rigidbody.IsGrounded)
                rigidbody.AddForce(new(0.0f, 3.0f, 0.0f));

            if (movement != Vector3f.Zero)
            {
                movement = Vector3f.Normalize(movement);

            GameObject.Transform.LocalPosition += movement * InputManager.DeltaTime;
        }
    }
}
