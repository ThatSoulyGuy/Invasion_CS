using Invasion.Math;
using Invasion.World;
using System;

namespace Invasion.Util
{
    public static class CoordinateHelper
    {
        public static Vector3i WorldToChunkCoordinates(Vector3f worldPosition)
        {
            int x = DivFloor((int)MathF.Floor(worldPosition.X), Chunk.CHUNK_SIZE);
            int y = DivFloor((int)MathF.Floor(worldPosition.Y), Chunk.CHUNK_SIZE);
            int z = DivFloor((int)MathF.Floor(worldPosition.Z), Chunk.CHUNK_SIZE);

            return new Vector3i(x, y, z);
        }

        private static int DivFloor(int a, int b)
        {
            int result = a / b;
            if (a % b < 0)
                result--;
            return result;
        }

        public static Vector3f ChunkToWorldCoordinates(Vector3i chunkPosition)
        {
            return new(chunkPosition.X * Chunk.CHUNK_SIZE, chunkPosition.Y * Chunk.CHUNK_SIZE, chunkPosition.Z * Chunk.CHUNK_SIZE);
        }

        public static Vector3i WorldToBlockCoordinates(Vector3f worldPosition)
        {
            int x = (int)MathF.Floor(worldPosition.X);
            int y = (int)MathF.Floor(worldPosition.Y);
            int z = (int)MathF.Floor(worldPosition.Z);

            x = Mod(x, Chunk.CHUNK_SIZE);
            y = Mod(y, Chunk.CHUNK_SIZE);
            z = Mod(z, Chunk.CHUNK_SIZE);

            return new Vector3i(x, y, z);
        }

        private static int Mod(int a, int n)
        {
            int result = a % n;

            if (result < 0) 
                result += n;

            return result;
        }

        public static Vector3f BlockToWorldCoordinates(Vector3i blockPosition, Vector3i chunkPosition)
        {
            Vector3f worldPosition = new Vector3f(
                chunkPosition.X * Chunk.CHUNK_SIZE + blockPosition.X,
                chunkPosition.Y * Chunk.CHUNK_SIZE + blockPosition.Y,
                chunkPosition.Z * Chunk.CHUNK_SIZE + blockPosition.Z
            );
            return worldPosition;
        }
    }
}
