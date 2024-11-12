using Invasion.Audio;
using Invasion.Block;
using Invasion.Core;
using Invasion.Entity.Models;
using Invasion.Math;
using Invasion.World;
using Invasion.World.SpawnManagers;
using System.Threading.Tasks;

namespace Invasion.Entity.Entities
{
    public class EntityGoober : AIEntity
    {
        public override string RegistryName => "entity_goober";

        public override Vector3f ColliderSpecification { get; } = new(0.6f, 0.8f, 0.6f);

        private ModelGoober Model => GameObject.GetComponent<ModelGoober>()!;

        private float TimeSinceLastAttack = 0.0f;
        private readonly float AttackInterval = 1.5f;
        private readonly float DamageAmount = 10.0f;
 
        public EntityGoober() : base(30.0f, 0.1f, 0.2f) { }

        public override void Initialize()
        {
            base.Initialize();

            GameObject.GetChild("Model").Transform.LocalPosition = new(0.0f, -0.287f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalRotation = new(0.0f, 180.0f, 0.0f);
            GameObject.GetChild("Model").Transform.LocalScale = new(0.062f);
        }

        public override void Update()
        {
            base.Update();

            if (IsDead)
                return;

            Rigidbody rigidbody = GameObject.GetComponent<Rigidbody>()!;

            if (Model != null)
                Model.GetPart("head").GameObject.Transform.LocalRotation = LookAt(Model.GetPart("head").GameObject.Transform.WorldPosition, InvasionMain.Player.Transform.WorldPosition);

            if (Model != null)
                GameObject.Transform.LocalRotation = new(0.0f, Model.GetPart("head").GameObject.Transform.LocalRotation.Y, 0.0f);

            float distanceToPlayer = Vector3f.Distance(GameObject.Transform.WorldPosition, InvasionMain.Player.Transform.WorldPosition);

            if (distanceToPlayer < 4)
            {
                TimeSinceLastAttack += Time.DeltaTime;

                if (TimeSinceLastAttack >= AttackInterval)
                {
                    InvasionMain.Player.GetComponent<EntityPlayer>().Damage(20);
                    TimeSinceLastAttack = 0.0f;
                }
            }

            if (rigidbody.IsGrounded && distanceToPlayer > 1.5)
                rigidbody.Move(GameObject.Transform.Forward * Time.DeltaTime, 4.5f);

            if (rigidbody.IsGrounded)
            {
                Vector3f forward = GameObject.Transform.Forward;
                forward.Y = 1;

                rigidbody.AddForce(new Vector3f(0.0f, 10.0f, 0.0f) * forward);
                InvasionMain.Overworld.GetComponent<IWorld>().SetBlock(GameObject.Transform.WorldPosition, BlockList.DIRT, true);

                AudioSource source = AudioSource.Create("goober_jump", false, new("Audio/Entity_Bounce.wav", "Invasion"));

                source.Volume = 0.85f;

                source.Play();
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

            SpawnManagerGoober.GooberEntities.Remove(GameObject);

            InvasionMain.Overworld.GetComponent<IWorld>().KillEntity(this);
        }
    }

}
