using Invasion.Audio;
using Invasion.Block;
using Invasion.Core;
using Invasion.Entity.Models;
using Invasion.Math;
using Invasion.Util;
using Invasion.World;
using Invasion.World.SpawnManagers;
using System;
using System.Threading.Tasks;

namespace Invasion.Entity.Entities
{
    public class EntityBoss : AIEntity
    {
        public override string RegistryName => "entity_boss";
        public override Vector3f ColliderSpecification { get; } = new(5.0f, 10.0f, 5.0f);

        private ModelBoss Model => GameObject.GetComponent<ModelBoss>()!;
        private float ArmLegWaveFrequency = 1.5f;
        private float AttackRange = 10.0f;
        private float MoveSpeed = 16.0f;
        private float JumpForce = 10.0f;
        private bool IsAttacking = false;

        public EntityBoss() : base(300.0f, 0.1f, 0.2f) { }

        public override void Initialize()
        {
            base.Initialize();
            GameObject.GetChild("Model").Transform.LocalPosition = new(0.0f, -5.287f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalRotation = new(0.0f, 180.0f, 0.0f);

            Rigidbody rigidbody = GameObject.GetComponent<Rigidbody>()!;

            rigidbody.UseGravity = true;
        }

        public override void Update()
        {
            base.Update();

            if (IsDead)
                return;

            Rigidbody rigidbody = GameObject.GetComponent<Rigidbody>()!;

            var playerPosition = InvasionMain.Player.Transform.WorldPosition;
            var bossPosition = GameObject.Transform.WorldPosition;
            
            Vector3f directionToPlayer = new Vector3f(
                playerPosition.X - bossPosition.X,
                0.0f,
                playerPosition.Z - bossPosition.Z
            );

            if (GameObject.Transform.LocalPosition.Y <= -8.0f)
            {
                rigidbody.AddForce(new Vector3f(0.0f, 200.0f, 0.0f) + new Vector3f(directionToPlayer.X * (MoveSpeed / 2), 0.0f, directionToPlayer.Z * (MoveSpeed / 2)));

                var audioSource = AudioSource.Create("boss_jump", false, new DomainedPath("Audio/Entity_Bounce.wav", "Invasion"));
                audioSource.Volume = 0.85f;
                audioSource.Play();
            }

            if (directionToPlayer.LengthSquared() > 0.0f)
                directionToPlayer = Vector3f.Normalize(directionToPlayer);

            if (directionToPlayer.LengthSquared() > 0.0f)
            {
                float angle = MathF.Atan2(directionToPlayer.X, directionToPlayer.Z) * (180.0f / MathF.PI);
                GameObject.Transform.LocalRotation = new(0.0f, angle, 0.0f);
            }

            if (Vector3f.Distance(new Vector3f(bossPosition.X, 0.0f, bossPosition.Z), new Vector3f(playerPosition.X, 0.0f, playerPosition.Z)) > AttackRange)
            {
                if (rigidbody.IsGrounded)
                    rigidbody.AddForce(new Vector3f(directionToPlayer.X * MoveSpeed, 0.0f, directionToPlayer.Z * MoveSpeed));
                
                IsAttacking = false;
            }
            else
            {
                if (!IsAttacking)
                {
                    InvasionMain.Player.GetComponent<EntityPlayer>().Damage(20);
                    IsAttacking = true;
                }
            }

            if (rigidbody.IsGrounded)
            {
                rigidbody.AddForce(new Vector3f(0.0f, JumpForce, 0.0f));

                var audioSource = AudioSource.Create("boss_jump", false, new DomainedPath("Audio/Entity_Bounce.wav", "Invasion"));
                audioSource.Volume = 0.85f;
                audioSource.Play();
            }

            if (Model != null)
            {
                float waveMagnitude = rigidbody.Magnitude * ArmLegWaveFrequency;
                float waveValue = MathF.Sin(Environment.TickCount * 0.001f) * waveMagnitude;

                Model.GetPart("left_arm").GameObject.Transform.LocalRotation = new(waveValue, 0.0f, 0.0f);
                Model.GetPart("right_arm").GameObject.Transform.LocalRotation = new(-waveValue, 0.0f, 0.0f);
                Model.GetPart("left_leg").GameObject.Transform.LocalRotation = new(-waveValue, 0.0f, 0.0f);
                Model.GetPart("right_leg").GameObject.Transform.LocalRotation = new(waveValue, 0.0f, 0.0f);
            }
        }

        public override async void OnDamaged(float amount)
        {
            base.OnDamaged(amount);

            Model?.ActivateDamageMeshes();

            await Task.Delay(200);

            Model?.DeactivateDamageMeshes();
        }

        public override void OnDeath()
        {
            base.OnDeath();

            SpawnManagerGoober.BossAlive = false;

            InvasionMain.Overworld.GetComponent<IWorld>().KillEntity(this);
        }
    }
}