using Invasion.Math;
using Invasion.World;
using System;

namespace Invasion.Util
{
    public static class CoordinateHelper
    {
        public static Vector3i WorldToChunkCoordinates(Vector3f worldPosition)
        {
            return new((int)MathF.Floor(worldPosition.X / Chunk.CHUNK_SIZE), (int)MathF.Floor(worldPosition.Y / Chunk.CHUNK_SIZE), (int)MathF.Floor(worldPosition.Z / Chunk.CHUNK_SIZE));
        }

        public static Vector3f ChunkToWorldCoordinates(Vector3i chunkPosition)
        {
            return new(chunkPosition.X * Chunk.CHUNK_SIZE, chunkPosition.Y * Chunk.CHUNK_SIZE, chunkPosition.Z * Chunk.CHUNK_SIZE);
        }
    }
}
