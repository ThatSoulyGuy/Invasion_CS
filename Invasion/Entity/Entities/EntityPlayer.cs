using System;
using System.Collections.Generic;
using System.IO;
using Invasion.Audio;
using Invasion.Block;
using Invasion.Core;
using Invasion.ECS;
using Invasion.Entity.Models;
using Invasion.Math;
using Invasion.Model;
using Invasion.Render;
using Invasion.UI;
using Invasion.UI.Elements;
using Invasion.UI.GUIs;
using Invasion.Util;
using Invasion.World;
using Invasion.World.SpawnManagers;

namespace Invasion.Entity.Entities
{
    public class EntityPlayer() : IEntity(100.0f, 6.0f, 12.0f)
    {
        public override string RegistryName => "entity_player";
        public float MouseSensitivity { get; set; } = 80.0f;

        public override Vector3f ColliderSpecification { get; } = new(0.6f, 1.89f, 0.6f);

        public UIText HealthText { get; private set; } = null!;
        public UIText InstructionsText { get; private set; } = null!;
        public UIText WaveCountText { get; private set; } = null!;
        public UIText YouDiedText { get; private set; } = null!;
        public UIText YouWinText { get; private set; } = null!;
        public UIText ScoreText { get; private set; } = null!;
        public UIImage CrosshairImage { get; private set; } = null!;

        public GameObject RenderCamera => GameObject.GetChild("Camera");

        private int SlotIndex { get; set; } = 0;
        private int Score { get; set; } = 0;
        private float blockTime = 0.0f;
        private float fireTime = 0.0f;

        public override void Initialize()
        {
            base.Initialize();

            InputManager.SetCursorMode(true);

            GameObject.AddChild(GameObject.Create("Camera"));

            HealthText = new("healthText", new("Font/Minecraft-Seven_v2.ttf", "Invasion"), new(10.0f, 0.0f), new(150.0f, 50.0f), Alignment.Bottom | Alignment.Left)
            {
                FontSize = 42.0f,
                Text = "Health: -1",
            };

            InstructionsText = new("instructions", new("Font/Minecraft-Seven_v2.ttf", "Invasion"), new(80.0f, 0.0f), new(300.0f, 160.0f), Alignment.Bottom | Alignment.Right)
            {
                FontSize = 40.0f,
                Text = "To switch from using your\n weapon to your hand, press 'C'!\nPress 'X' to close this dialog",
            };

            WaveCountText = new("waveCount", new("Font/Minecraft-Seven_v2.ttf", "Invasion"), new(10.0f, 0.0f), new(150.0f, 50.0f), Alignment.Top | Alignment.CenterX)
            {
                FontSize = 42.0f,
                Text = "Wave: -1",
            };

            YouDiedText = new("youDied", new("Font/Minecraft-Seven_v2.ttf", "Invasion"), new(0.0f, 0.0f), new(300.0f, 100.0f), Alignment.CenterX | Alignment.CenterY)
            {
                FontSize = 60.0f,
                Text = "You Died!",
            };

            YouWinText = new("youDied", new("Font/Minecraft-Seven_v2.ttf", "Invasion"), new(0.0f, 0.0f), new(300.0f, 100.0f), Alignment.CenterX | Alignment.CenterY)
            {
                FontSize = 60.0f,
                Text = "You Won!",
            };

            ScoreText = new("score", new("Font/Minecraft-Seven_v2.ttf", "Invasion"), new(0.0f, -50.0f), new(150.0f, 50.0f), Alignment.Top | Alignment.Left)
            {
                FontSize = 42.0f,
                Text = "Score: -1",
            };

            YouDiedText.IsVisible = false;
            YouWinText.IsVisible = false;

            CrosshairImage = new("crosshair", TextureManager.Get<Texture>("crosshair"), new(0.0f, 0.0f), new(32.0f, 32.0f), Alignment.CenterX | Alignment.CenterY);

            //GUIList.GUIs["hud"].IsActive = true;

            RenderCamera.AddComponent(Camera.Create(45.0f, 0.01f, 1000.0f));
            RenderCamera.Transform.LocalPosition = new(0.0f, 0.9f, 0.0f);

            GameObject laserGun = RenderCamera.AddChild(GameObject.Create("LaserGun"));

            laserGun.Transform.LocalPosition = new(1.0f, -0.8f, 1.0f);
            laserGun.Transform.LocalRotation = new(0.0f, -90.0f, 0.0f);
            laserGun.Transform.LocalScale = new(0.062f);
            
            ModelLoader.LoadModel(File.ReadAllText("Assets/Invasion/Model/Item/LaserGun.json"), out List<Vertex> vertices, out List<uint> indices);

            laserGun.AddComponent(ShaderManager.Get("default"));
            laserGun.AddComponent(TextureManager.Get<Texture>("laser_gun"));
            laserGun.AddComponent(Mesh.Create("lasergun", vertices, indices));

            laserGun.GetComponent<Mesh>().Generate();
        }

        public override void Update()
        {
            base.Update();

            if (SpawnManagerGoober.BossSpawned && !SpawnManagerGoober.BossAlive)
                Damage(float.NegativeInfinity);

            string healthText = $"Health: {Health}";

            if (HealthText.Text != healthText)
                HealthText.Text = healthText;

            string waveCountText;
            
            if (SpawnManagerGoober.BossSpawned)
                waveCountText = "Wave: FINAL";
            else
                waveCountText = $"Wave: {SpawnManagerGoober.WaveCount - 1}";

            if (WaveCountText.Text != waveCountText)
                WaveCountText.Text = waveCountText;

            string scoreText = $"Score: {Score}";

            if (ScoreText.Text != scoreText)
                ScoreText.Text = scoreText;

            UpdateControls();
            UpdateMouselook();
            UpdateMovement();

            //GUIList.GUIs["hud"].GetElement("hotbarSelector")!.AlignmentOffset = new(300 + (SlotIndex * 128), 0);

            blockTime += Time.DeltaTime;
            fireTime += Time.DeltaTime;
        }

        public override void OnDamaged(float amount)
        {
            base.OnDamaged(amount);

            AudioSource audio = AudioSource.Create("player_hurt", false, new("Audio/Player_Hurt.wav", "Invasion"));

            audio.Volume = 1.5f;

            audio.Play();
        }

        public override void OnDeath()
        {
            InputManager.SetCursorMode(false);

            if (SpawnManagerGoober.BossSpawned && !SpawnManagerGoober.BossAlive)
                YouWinText.IsVisible = true;
            else
                YouDiedText.IsVisible = true;

            GameObjectManager.Stop();
        }

        private void CreateRay(Vector3f start, Vector3f end, Vector3f color)
        {
            GameObject ray = GameObject.Create("Ray" + new Random().Next());

            ray.AddComponent(ShaderManager.Get("line"));
            ray.AddComponent(new DeleteAfter(4.0f));
            ray.AddComponent(LineMesh.Create("ray",
            [
                new Vertex(start, color, Vector3f.Zero, Vector2f.Zero),
                new Vertex(end, color, Vector3f.Zero, Vector2f.Zero)
            ], 
            [
                0, 1
            ]));

            ray.GetComponent<LineMesh>().Generate();
        }

        private void UpdateControls()
        {
            if (InputManager.MouseLeftPressed)
            {
                if (RenderCamera.GetChild("LaserGun").Active && fireTime >= 0.19f)
                {
                    var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, RenderCamera.Transform.Forward, 40.0f, GameObject.GetComponent<BoundingBox>());

#if DEBUG
                    if (hit && information.Collider.GameObject == null)
                        CreateRay(RenderCamera.Transform.WorldPosition, information.HitPoint, new(0.0f, 1.0f, 0.0f));
                    else if (hit && information.Collider.GameObject != null)
                        CreateRay(RenderCamera.Transform.WorldPosition, information.HitPoint, new(0.0f, 0.0f, 1.0f));
                    else
                        CreateRay(RenderCamera.Transform.WorldPosition, RenderCamera.Transform.WorldPosition + RenderCamera.Transform.Forward * 40.0f, new(1.0f, 0.0f, 0.0f));
#endif

                    Vector3f position = RenderCamera.Transform.WorldPosition;

                    position += RenderCamera.Transform.Forward * 0.5f;

                    //InvasionMain.Overworld.GetComponent<IWorld>().SpawnEntity<EntityLaserBeam, ModelLaserBeam>(new EntityLaserBeam(RenderCamera.Transform.Forward, 2.0f), position);

                    if (hit && information.Collider!.GameObject != null && information.Collider.GameObject.HasComponent<EntityGoober>())
                    {
                        if (information.Collider.GameObject.GetComponent<EntityGoober>().Health <= 10)
                            Score += 10 + SpawnManagerGoober.WaveCount * 2;

                        information.Collider.GameObject.GetComponent<EntityGoober>().Damage(10);
                    }
                    else if (hit && information.Collider!.GameObject != null && information.Collider.GameObject.HasComponent<EntityBoss>())
                    {
                        if (information.Collider.GameObject.GetComponent<EntityBoss>().Health <= 10)
                            Score += 100;

                        information.Collider.GameObject.GetComponent<EntityBoss>().Damage(10);
                    }

                    AudioSource audio = AudioSource.Create("laser", false, new("Audio/LaserGun_Fire.wav", "Invasion"));

                    audio.Play();

                    fireTime = 0.0f;
                }
                else if(!RenderCamera.GetChild("LaserGun").Active && blockTime >= 0.1f)
                {
                    var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, RenderCamera.Transform.Forward, 10.0f, GameObject.GetComponent<BoundingBox>());

                    Vector3f position = information.HitPoint;

                    position -= information.Normal * 0.5f;

                    if (hit)
                        InvasionMain.Overworld.GetComponent<IWorld>().SetBlock(position, BlockList.AIR);

                    blockTime = 0.0f;
                }
                
            }

            if (InputManager.MouseRightPressed && blockTime >= 0.1f)
            {
                var (hit, information) = Raycast.Cast(RenderCamera.Transform.WorldPosition, RenderCamera.Transform.Forward, 10.0f, GameObject.GetComponent<BoundingBox>());

                Vector3f position = information.HitPoint;

                position += information.Normal * 0.5f;

                if (hit)
                    InvasionMain.Overworld.GetComponent<IWorld>().SetBlock(position, BlockList.WOOD, true);

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

            if (InputManager.MouseWheelDelta > 0)
            {
                SlotIndex++;

                if (SlotIndex > 7)
                    SlotIndex = 0;
            }
            else if (InputManager.MouseWheelDelta < 0)
            {
                SlotIndex--;

                if (SlotIndex < 0)
                    SlotIndex = 7;
            }

            if (InputManager.GetKey(KeyCode.Z, KeyState.Pressed))
                GUIList.GUIs["hud"].GetElement("hotbarSelector")!.Position += new Vector2f(1, 0);
        }

        private void UpdateMouselook()
        {
            Vector2f mouseMovement = InputManager.GetMouseMovementOffsets();

            mouseMovement *= MouseSensitivity;
            mouseMovement *= Time.DeltaTime;

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

            rigidbody.Move(movement * Time.DeltaTime, InputManager.GetKeyHeld(KeyCode.LeftShift) ? RunningSpeed : WalkingSpeed);
        }
    }
}
