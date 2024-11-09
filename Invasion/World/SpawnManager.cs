using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invasion.World
{
    public abstract class SpawnManager
    {
        public abstract void OnUpdateTick(IWorld world);
        public abstract void OnSpawnTick(IWorld world, List<Chunk> chunks);
    }
}
