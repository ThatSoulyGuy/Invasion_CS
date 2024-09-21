using Invasion.World;
using System;
using System.Numerics;

namespace Invasion.Util
{
    public static class CoordinateHelper
    {
        public static Vector3 WorldToChunkCoordinates(Vector3 worldPosition)
        {
            return new Vector3((int)MathF.Floor(worldPosition.X / Chunk.CHUNK_SIZE), (int)MathF.Floor(worldPosition.Y / Chunk.CHUNK_SIZE), (int)MathF.Floor(worldPosition.Z / Chunk.CHUNK_SIZE));
        }

        public static Vector3 ChunkToWorldCoordinates(Vector3 chunkPosition)
        {
            return new Vector3(chunkPosition.X * Chunk.CHUNK_SIZE, chunkPosition.Y * Chunk.CHUNK_SIZE, chunkPosition.Z * Chunk.CHUNK_SIZE);
        }
    }
}
