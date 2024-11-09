using Invasion.ECS;
using Invasion.Entity.Entities;
using Invasion.Entity.Models;
using Invasion.Math;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Invasion.World.SpawnManagers
{
    public class SpawnManagerGoober : SpawnManager
    {
        public static short WaveCount { get; set; } = 0;
        public static List<GameObject> GooberEntities { get; } = [];
        public static bool BossSpawned { get; private set; } = false;
        public static bool BossAlive { get; set; } = false;

        public override void OnUpdateTick(IWorld world)
        {
            if (GooberEntities.Count == 0)
            {
                if (WaveCount < 10)
                    WaveCount += 1;
                else if (!BossSpawned)
                {
                    SpawnFinalBoss(world);
                    BossSpawned = true;
                    return;
                }
                else
                    return;

                OnSpawnTick(world, [.. world.GetLoadedChunks().Values]);
            }
        }

        public override void OnSpawnTick(IWorld world, List<Chunk> loadedChunks)
        {
            if (GooberEntities.Count >= WaveCount * 2)
                return;

            int count = 0;

            foreach (var chunk in loadedChunks)
            {
                if (WaveCount < 10)
                {
                    if (count >= WaveCount * 2)
                        return;
                }
                else
                {
                    if (count >= 30)
                        return;
                }

                Vector3f position = new(chunk.GameObject.Transform.WorldPosition.X, 60, chunk.GameObject.Transform.WorldPosition.Z);
                EntityGoober goober = world.SpawnEntity<EntityGoober, ModelGoober>(position).GetComponent<EntityGoober>();

                goober.Health = WaveCount * 10;
                GooberEntities.Add(goober.GameObject);
                count++;
            }
        }

        private void SpawnFinalBoss(IWorld world)
        {
            Vector3f bossPosition = new(0, 60, 0); 

            EntityBoss finalBoss = world.SpawnEntity<EntityBoss, ModelBoss>(bossPosition).GetComponent<EntityBoss>();
            finalBoss.Health = 100;

            BossAlive = true;

            Console.WriteLine("Final Boss Spawned with 300 health!");
        }
    }
}
