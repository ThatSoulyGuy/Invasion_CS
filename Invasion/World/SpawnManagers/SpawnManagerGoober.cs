using Invasion.ECS;
using Invasion.Entity.Entities;
using Invasion.Entity.Models;
using Invasion.Math;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Invasion.World.SpawnManagers
{
    public class SpawnManagerGoober : SpawnManager
    {
        public static byte WaveCount { get; set; } = 1;

        public static List<GameObject> GooberEntities { get; } = [];

        public override void OnSpawnTick(IWorld world, List<Chunk> loadedChunks)
        {
            if (GooberEntities.Count == 0)
                WaveCount += 1;

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

                GooberEntities.Add(world.SpawnEntity<EntityGoober, ModelGoober>(position));

                count++;
            }
        }
    }
}
