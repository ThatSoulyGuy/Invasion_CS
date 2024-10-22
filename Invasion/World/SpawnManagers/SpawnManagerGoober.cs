﻿using Invasion.Entity.Entities;
using Invasion.Entity.Models;
using Invasion.Math;
using System.Collections.Generic;

namespace Invasion.World.SpawnManagers
{
    public class SpawnManagerGoober : SpawnManager
    {
        public override void OnSpawnTick(IWorld world, List<Chunk> loadedChunks)
        {
            foreach (var chunk in loadedChunks)
            {
                Vector3f position = new(chunk.GameObject.Transform.WorldPosition.X, 60, chunk.GameObject.Transform.WorldPosition.Z);

                world.SpawnEntity<EntityGoober, ModelGoober>(position);
            }
        }
    }
}