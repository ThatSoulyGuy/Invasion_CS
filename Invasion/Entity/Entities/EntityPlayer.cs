using System;
using System.Collections.Generic;
using System.IO;
using Invasion.Block;
using Invasion.Core;
using Invasion.ECS;
using Invasion.Entity.Models;
using Invasion.Math;
using Invasion.Model;
using Invasion.Render;
using Invasion.UI.Elements;
using Invasion.World;

namespace Invasion.Entity.Entities
{
    public class EntityPlayer() : IEntity(100.0f, 6.0f, 12.0f)
    {
        public override string RegistryName => "entity_player";
        public float MouseSensitivity { get; set; } = 80.0f;

        public override BoundingBox ColliderSpecification { get; } = BoundingBox.Create(new(0.6f, 1.89f, 0.6f));

        public UIText HealthText { get; private set; } = null!;
        public UIText InstructionsText { get; private set; } = null!;
        public GameObject RenderCamera => GameObject.GetChild("Camera");

        private float blockTime = 0.0f;

        public override void Initialize()
        {
            base.Initialize();

            InputManager.SetCursorMode(true);

            GameObject.AddChild(GameObject.Create("Camera"));

            HealthText = new("healthText", new("Font/Segoeuithis.ttf", "Invasion"), new(80.0f, 0.0f), new(150.0f, 50.0f))
            {
                Text = "Health: -1",
            };

            InstructionsText = new("instructions", new("Font/Segoeuithis.ttf", "Invasion"), new(720.0f, 120.0f), new(450.0f, 160.0f))
            {
                Text = "To switch from using your\n weapon to your hand, press 'C'!\nPress 'X' to close this dialog",
            };

            RenderCamera.AddComponent(Camera.Create(45.0f, 0.01f, 1000.0f));
            RenderCamera.Transform.LocalPosition = new(0.0f, 0.9f, 0.0f);

            GameObject laserGun = RenderCamera.AddChild(GameObject.Create("LaserGun"));

            laserGun.Transform.LocalPosition = new(1.0f, -0.8f, 1.0f);
            laserGun.Transform.LocalRotation = new(0.0f, -90.0f, 0.0f);
            laserGun.Transform.LocalScale = new(0.062f);
            
            ModelLoader.LoadModel(File.ReadAllText("Assets/Invasion/Model/Item/LaserGun.json"), out List<Vertex> vertices, out List<uint> indices);

            laserGun.AddComponent(ShaderManager.Get("default"));
            laserGun.AddComponent(TextureManager.Get("laser_gun"));
            laserGun.AddComponent(Mesh.Create("lasergun", vertices, indices));

            laserGun.GetComponent<Mesh>().Generate();
        }

        public override void Update()
        {
            base.Update();

            string healthText = $"Health: {Health}";

            if (HealthText.Text != healthText)
                HealthText.Text = healthText;

            UpdateControls();
            UpdateMouselook();
            UpdateMovement();

            blockTime += InputManager.DeltaTime;
        }

        public override void OnDeath()
        {
            GameObjectManager.Unregister(GameObject.Name);
        }

        private void UpdateControls()
        {
            var cameraWorldPosition = RenderCamera.Transform.WorldPosition;
            var cameraForward = RenderCamera.Transform.Forward;

            if (InputManager.MouseLeftPressed && blockTime >= 0.1f)
            {
                if (RenderCamera.GetChild("LaserGun").Active)
                {
                    var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, RenderCamera.Transform.Forward, 40.0f, GameObject.GetComponent<BoundingBox>());

                    Vector3f position = information.HitPoint;

                    position += information.Normal * 0.5f;
                    
                    //InvasionMain.Overworld.GetComponent<IWorld>().SpawnEntity<EntityLaserBeam, ModelLaserBeam>(new EntityLaserBeam(RenderCamera.Transform.Forward, 2.0f), RenderCamera.Transform.WorldPosition);

                    Console.WriteLine("1 " + (information.Collider.GameObject != null));    

                    if (information.Collider.GameObject != null)
                        Console.WriteLine("2 " + information.Collider.GameObject.HasComponent<EntityGoober>());

                    if (hit && information.Collider.GameObject != null && information.Collider.GameObject.HasComponent<EntityGoober>())
                        information.Collider.GameObject.GetComponent<EntityGoober>().Health -= 10;
                }
                else
                {
                    var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, RenderCamera.Transform.Forward, 10.0f, GameObject.GetComponent<BoundingBox>());

                    Vector3f position = information.HitPoint;

                    position += information.Normal * 0.5f;

                    if (hit)
                        InvasionMain.Overworld.GetComponent<IWorld>().SetBlock(position, BlockList.AIR);

                    blockTime = 0.0f;
                }
                
            }

            if (InputManager.MouseRightPressed && blockTime >= 0.1f)
            {
                var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, RenderCamera.Transform.Forward, 10.0f, GameObject.GetComponent<BoundingBox>());

                Vector3f position = information.HitPoint;

                position -= information.Normal * 0.5f;

                if (hit)
                    InvasionMain.Overworld.GetComponent<IWorld>().SetBlock(position, BlockList.BEDROCK, true);

                blockTime = 0.0f;
            }

            if (InputManager.GetKey(KeyCode.C, KeyState.Pressed))
            {
                GameObject laserGun = RenderCamera.GetChild("LaserGun");

                if (laserGun.Active)
                    laserGun.Active = false;
                else
                    laserGun.Active = true;
            }

            if (InputManager.GetKey(KeyCode.X, KeyState.Pressed))
                InstructionsText.Text = string.Empty;
        }

        private void UpdateMouselook()
        {
            Vector2f mouseMovement = InputManager.GetMouseMovementOffsets();

            mouseMovement *= MouseSensitivity;
            mouseMovement *= InputManager.DeltaTime;

            RenderCamera.Transform.Rotate(new(mouseMovement.Y, mouseMovement.X, 0.0f));

            RenderCamera.Transform.LocalRotation = new(
                Vortice.Mathematics.MathHelper.Clamp(RenderCamera.Transform.LocalRotation.X, -89.0f, 89.0f),
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
                rigidbody.AddForce(new(0.0f, 6.0f, 0.0f));

            if (movement != Vector3f.Zero)
                movement = Vector3f.Normalize(movement);

            if (!rigidbody.IsGrounded)
                movement *= 0.15f;

            rigidbody.Move(movement * InputManager.DeltaTime, InputManager.GetKeyHeld(KeyCode.LeftShift) ? RunningSpeed : WalkingSpeed);
        }
    }
}
