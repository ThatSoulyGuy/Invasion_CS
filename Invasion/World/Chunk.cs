using Invasion.Block;
using Invasion.ECS;
using Invasion.Math;
using Invasion.Render;
using Invasion.Util;
using System;
using System.Collections.Generic;

namespace Invasion.World
{
    public class Chunk : Component
    {
        public const byte CHUNK_SIZE = 16;

        public Dictionary<Vector3i, short> Blocks { get; private set; } = new();

        private List<Vertex> Vertices { get; set; } = new();
        private List<uint> Indices { get; set; } = new();

        private Dictionary<Vector3i, BoundingBox> Colliders { get; } = new();

        private HashSet<Vector3i> DirtyBlocks { get; set; } = new();

        private class BlockMeshData
        {
            public List<Vertex> Vertices = new();
            public List<uint> Indices = new();
        }

        private Dictionary<Vector3i, BlockMeshData> BlockMeshDataMap { get; } = new();

        private static readonly Vector3i[] FaceNormals =
        {
        new Vector3i( 0,  0,  1), // Front
        new Vector3i( 0,  0, -1), // Back
        new Vector3i( 1,  0,  0), // Right
        new Vector3i(-1,  0,  0), // Left
        new Vector3i( 0,  1,  0), // Top
        new Vector3i( 0, -1,  0), // Bottom
    };

        public bool NeedsRegeneration { get; private set; } = false;

        // Track the last update cycle
        public int LastUpdatedCycle { get; set; } = -1;

        private Chunk() { }

        private void Initialize(bool generateNothing)
        {
            if (!generateNothing)
            {
                for (int x = 0; x < CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        for (int z = 0; z < CHUNK_SIZE; z++)
                        {
                            short block = (y == CHUNK_SIZE - 1) ? BlockList.GRASS :
                                          (y == 0) ? BlockList.BEDROCK :
                                          (y > 11) ? BlockList.DIRT : BlockList.STONE;
                            Vector3i position = new Vector3i(x, y, z);
                            Blocks[position] = block;
                        }
                    }
                }

                MarkAllBlocksDirty();
            }
        }

        private void MarkAllBlocksDirty()
        {
            foreach (var position in Blocks.Keys)
                DirtyBlocks.Add(position);

            NeedsRegeneration = true;
        }

        public void Generate(int currentUpdateCycle)
        {
            if (DirtyBlocks.Count == 0 || LastUpdatedCycle >= currentUpdateCycle)
                return;

            RemoveVerticesAndIndicesForDirtyBlocks();

            TextureAtlas atlas = GameObject.GetComponent<TextureAtlas>();

            foreach (var position in DirtyBlocks)
            {
                if (!Blocks.TryGetValue(position, out short block))
                {
                    if (Colliders.TryGetValue(position, out BoundingBox? collider))
                    {
                        collider.CleanUp();
                        Colliders.Remove(position);
                    }

                    continue;
                }

                BlockData blockData = BlockList.GetBlockData(block);

                BlockMeshData blockMeshData = new();

                for (int i = 0; i < FaceNormals.Length; i++)
                {
                    Vector3i normal = FaceNormals[i];

                    if (IsFaceExposed(position, normal))
                    {
                        if (!Colliders.ContainsKey(position))
                        {
                            Vector3f worldPosition = CoordinateHelper.BlockToWorldCoordinates(position, CoordinateHelper.WorldToChunkCoordinates(GameObject.Transform.WorldPosition));
                            worldPosition += new Vector3f(0.5f, 0.5f, 0.5f);

                            Colliders[position] = BoundingBox.Create(worldPosition, Vector3f.One);
                        }

                        AddFaceToBlockMeshData(blockMeshData, position, normal, blockData, atlas, i);
                    }
                }

                BlockMeshDataMap[position] = blockMeshData;
            }

            RebuildMesh();

            DirtyBlocks.Clear();
            NeedsRegeneration = false;
            LastUpdatedCycle = currentUpdateCycle;
        }

        private void RemoveVerticesAndIndicesForDirtyBlocks()
        {
            foreach (var position in DirtyBlocks)
            {
                BlockMeshDataMap.Remove(position);

                if (Colliders.TryGetValue(position, out BoundingBox? collider))
                {
                    collider.CleanUp();
                    Colliders.Remove(position);
                }
            }
        }

        private void AddFaceToBlockMeshData(BlockMeshData blockMeshData, Vector3i position, Vector3i normal, BlockData blockData, TextureAtlas atlas, int faceIndex)
        {
            int vertexStartIndex = blockMeshData.Vertices.Count;

            Vector3f[] faceVertices = GetFaceVertices(position, faceIndex);

            Vector3f color = GetFaceColor(normal, blockData);
            Vector2f[] uv = GetFaceUV(blockData, normal, atlas);

            Vector3f normalf = normal;

            blockMeshData.Vertices.Add(new Vertex(faceVertices[0], color, normalf, uv[0]));
            blockMeshData.Vertices.Add(new Vertex(faceVertices[1], color, normalf, uv[1]));
            blockMeshData.Vertices.Add(new Vertex(faceVertices[2], color, normalf, uv[2]));
            blockMeshData.Vertices.Add(new Vertex(faceVertices[3], color, normalf, uv[3]));

            blockMeshData.Indices.Add((uint)(vertexStartIndex + 0));
            blockMeshData.Indices.Add((uint)(vertexStartIndex + 1));
            blockMeshData.Indices.Add((uint)(vertexStartIndex + 2));
            blockMeshData.Indices.Add((uint)(vertexStartIndex + 0));
            blockMeshData.Indices.Add((uint)(vertexStartIndex + 2));
            blockMeshData.Indices.Add((uint)(vertexStartIndex + 3));
        }

        private void RebuildMesh()
        {
            Vertices.Clear();
            Indices.Clear();

            uint vertexOffset = 0;

            foreach (var kvp in BlockMeshDataMap)
            {
                BlockMeshData blockMeshData = kvp.Value;

                Vertices.AddRange(blockMeshData.Vertices);

                foreach (uint index in blockMeshData.Indices)
                    Indices.Add(index + vertexOffset);

                vertexOffset += (uint)blockMeshData.Vertices.Count;
            }

            Mesh mesh = GameObject.GetComponent<Mesh>();

            mesh.Vertices = new(Vertices);
            mesh.Indices = new(Indices);

            if (Vertices.Count > 0 && Indices.Count > 0)
                mesh.Generate();
        }

        public void SetBlock(Vector3i position, short block)
        {
            if (IsValidPosition(position))
            {
                if (block == BlockList.AIR)
                    Blocks.Remove(position);
                else
                    Blocks[position] = block;

                MarkBlockDirty(position);

                if (IsEdgeBlock(position))
                    NotifyNeighborsToMarkDirty(position);
            }
        }

        private void MarkBlockDirty(Vector3i position)
        {
            DirtyBlocks.Add(position);

            foreach (var normal in FaceNormals)
            {
                Vector3i adjacentPosition = position + normal;

                if (IsValidPosition(adjacentPosition))
                    DirtyBlocks.Add(adjacentPosition);
            }

            NeedsRegeneration = true;
        }

        private bool IsValidPosition(Vector3i position)
        {
            return position.X >= 0 && position.X < CHUNK_SIZE &&
                   position.Y >= 0 && position.Y < CHUNK_SIZE &&
                   position.Z >= 0 && position.Z < CHUNK_SIZE;
        }

        private bool IsEdgeBlock(Vector3i position)
        {
            return position.X == 0 || position.X == CHUNK_SIZE - 1 ||
                   position.Y == 0 || position.Y == CHUNK_SIZE - 1 ||
                   position.Z == 0 || position.Z == CHUNK_SIZE - 1;
        }

        private void NotifyNeighborsToMarkDirty(Vector3i position)
        {
            IWorld world = InvasionMain.Overworld.GetComponent<IWorld>();
            int currentUpdateCycle = world.CurrentUpdateCycle;

            Vector3i chunkPosition = CoordinateHelper.WorldToChunkCoordinates(GameObject.Transform.WorldPosition);

            foreach (var normal in FaceNormals)
            {
                Vector3i adjacentPosition = position + normal;

                if (!IsValidPosition(adjacentPosition))
                {
                    Vector3i neighborChunkOffset = new Vector3i(
                        (adjacentPosition.X < 0) ? -1 : (adjacentPosition.X >= CHUNK_SIZE ? 1 : 0),
                        (adjacentPosition.Y < 0) ? -1 : (adjacentPosition.Y >= CHUNK_SIZE ? 1 : 0),
                        (adjacentPosition.Z < 0) ? -1 : (adjacentPosition.Z >= CHUNK_SIZE ? 1 : 0));

                    Vector3i neighborChunkPosition = chunkPosition + neighborChunkOffset;

                    Vector3i neighborBlockPosition = new Vector3i(
                        (adjacentPosition.X + CHUNK_SIZE) % CHUNK_SIZE,
                        (adjacentPosition.Y + CHUNK_SIZE) % CHUNK_SIZE,
                        (adjacentPosition.Z + CHUNK_SIZE) % CHUNK_SIZE);

                    if (world.GetLoadedChunks().TryGetValue(neighborChunkPosition, out Chunk? neighborChunk))
                    {
                        if (neighborChunk.LastUpdatedCycle < currentUpdateCycle)
                        {
                            neighborChunk.MarkBlockDirty(neighborBlockPosition);
                            neighborChunk.NeedsRegeneration = true;
                        }
                    }
                }
            }
        }

        private bool IsFaceExposed(Vector3i position, Vector3i normal)
        {
            Vector3i adjacentPosition = position + normal;

            if (IsValidPosition(adjacentPosition))
            {
                return !Blocks.ContainsKey(adjacentPosition) || Blocks[adjacentPosition] == BlockList.AIR;
            }
            else
            {
                Vector3i chunkPosition = CoordinateHelper.WorldToChunkCoordinates(GameObject.Transform.WorldPosition);

                Vector3i neighborChunkOffset = new Vector3i(
                    (adjacentPosition.X < 0) ? -1 : (adjacentPosition.X >= CHUNK_SIZE ? 1 : 0),
                    (adjacentPosition.Y < 0) ? -1 : (adjacentPosition.Y >= CHUNK_SIZE ? 1 : 0),
                    (adjacentPosition.Z < 0) ? -1 : (adjacentPosition.Z >= CHUNK_SIZE ? 1 : 0));

                Vector3i neighborChunkPosition = chunkPosition + neighborChunkOffset;

                Vector3i neighborBlockPosition = new Vector3i(
                    (adjacentPosition.X + CHUNK_SIZE) % CHUNK_SIZE,
                    (adjacentPosition.Y + CHUNK_SIZE) % CHUNK_SIZE,
                    (adjacentPosition.Z + CHUNK_SIZE) % CHUNK_SIZE);

                IWorld world = InvasionMain.Overworld.GetComponent<IWorld>();

                if (world.GetLoadedChunks().TryGetValue(neighborChunkPosition, out Chunk? neighborChunk))
                    return !neighborChunk.Blocks.ContainsKey(neighborBlockPosition) ||
                           neighborChunk.Blocks[neighborBlockPosition] == BlockList.AIR;
                else
                    return true;
            }
        }

        private Vector3f GetFaceColor(Vector3i normal, BlockData blockData)
        {
            if (normal == new Vector3i(0, 1, 0))
                return blockData.TopColor;
            else if (normal == new Vector3i(0, -1, 0))
                return new(1.0f, 1.0f, 1.0f);
            else
                return new(1.0f, 1.0f, 1.0f);
        }

        private Vector2f[] GetFaceUV(BlockData blockData, Vector3i normal, TextureAtlas atlas)
        {
            if (normal == new Vector3i(0, 1, 0))
                return atlas.GetTextureCoordinates(blockData.Textures["top"]);
            else if (normal == new Vector3i(0, -1, 0))
                return atlas.GetTextureCoordinates(blockData.Textures["bottom"]);
            else
                return atlas.GetTextureCoordinates(blockData.Textures["side"]);
        }

        private Vector3f[] GetFaceVertices(Vector3i position, int faceIndex)
        {
            Vector3f blockPosition = position;

            return faceIndex switch
            {
                0 => new[] { blockPosition + new Vector3f(0, 0, 1), blockPosition + new Vector3f(1, 0, 1), blockPosition + new Vector3f(1, 1, 1), blockPosition + new Vector3f(0, 1, 1) },
                1 => new[] { blockPosition + new Vector3f(1, 0, 0), blockPosition + new Vector3f(0, 0, 0), blockPosition + new Vector3f(0, 1, 0), blockPosition + new Vector3f(1, 1, 0) },
                2 => new[] { blockPosition + new Vector3f(1, 0, 1), blockPosition + new Vector3f(1, 0, 0), blockPosition + new Vector3f(1, 1, 0), blockPosition + new Vector3f(1, 1, 1) },
                3 => new[] { blockPosition + new Vector3f(0, 0, 0), blockPosition + new Vector3f(0, 0, 1), blockPosition + new Vector3f(0, 1, 1), blockPosition + new Vector3f(0, 1, 0) },
                4 => new[] { blockPosition + new Vector3f(0, 1, 1), blockPosition + new Vector3f(1, 1, 1), blockPosition + new Vector3f(1, 1, 0), blockPosition + new Vector3f(0, 1, 0) },
                5 => new[] { blockPosition + new Vector3f(0, 0, 0), blockPosition + new Vector3f(1, 0, 0), blockPosition + new Vector3f(1, 0, 1), blockPosition + new Vector3f(0, 0, 1) },
                _ => throw new ArgumentOutOfRangeException(nameof(faceIndex)),
            };
        }

        public override void CleanUp()
        {
            Vertices.Clear();
            Indices.Clear();

            foreach (var collider in Colliders.Values)
                collider.CleanUp();

            Colliders.Clear();
        }

        public static Chunk Create(bool generateNothing = false)
        {
            Chunk chunk = new();
            chunk.Initialize(generateNothing);
            return chunk;
        }
    }
}