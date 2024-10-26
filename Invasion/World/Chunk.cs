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

        public Dictionary<Vector3i, short> Blocks { get; private set; } = [];

        private List<Vertex> Vertices { get; set; } = [];
        private List<uint> Indices { get; set; } = [];

        private ConcurrentDictionary<Vector3i, BoundingBox> Colliders { get; } = [];

        private static Vector3f[] FaceNormals { get; } =
        {
            new Vector3f( 0,  0,  1),
            new Vector3f( 0,  0, -1),
            new Vector3f( 1,  0,  0),
            new Vector3f(-1,  0,  0),
            new Vector3f( 0,  1,  0),
            new Vector3f( 0, -1,  0),
        };

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
                            short block = (y == 15) ? BlockList.GRASS : (y == 0) ? BlockList.BEDROCK : (y > 11) ? BlockList.DIRT : BlockList.STONE;
                            Blocks[new Vector3i(x, y, z)] = block;
                        }
                    }
                }
            }
        }

        public void Generate()
        {
                Vertices.Clear();
                Indices.Clear();
            
                foreach (var collider in Colliders.Values)
                    collider.CleanUp();

                Colliders.Clear();

                TextureAtlas atlas = GameObject.GetComponent<TextureAtlas>();

                foreach (var kvp in Blocks)
                {
                    Vector3i position = kvp.Key;
                    short block = kvp.Value;

                    for (int i = 0; i < FaceNormals.Length; i++)
                    {
                        Vector3f normal = FaceNormals[i];

                        if (IsFaceExposed(position, normal))
                        {
                            if (!Colliders.ContainsKey(position))
                            {
                                Vector3f worldPosition = CoordinateHelper.BlockToWorldCoordinates(position, CoordinateHelper.WorldToChunkCoordinates(GameObject.Transform.WorldPosition));
                                worldPosition += new Vector3f(0.5f, 0.5f, 0.5f);

                                Colliders.TryAdd(position, BoundingBox.Create(worldPosition, Vector3f.One));
                            }

                                AddFace(position, normal, BlockList.GetBlockData(block), atlas, i);
                            }
                        }
                    }

                Mesh mesh = GameObject.GetComponent<Mesh>();

                mesh.Vertices = Vertices;
                mesh.Indices = Indices;

                if (Vertices.Count != 0 && Indices.Count != 0)
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

                Generate();
            }
        }

        private bool IsValidPosition(Vector3i position)
        {
            return position.X >= 0 && position.X < CHUNK_SIZE &&
                   position.Y >= 0 && position.Y < CHUNK_SIZE &&
                   position.Z >= 0 && position.Z < CHUNK_SIZE;
        }

        private bool IsFaceExposed(Vector3i position, Vector3f normal)
        {
            IWorld world = InvasionMain.Overworld.GetComponent<IWorld>();

            Vector3i adjacentPosition = position + new Vector3i(
                (int)MathF.Round(normal.X),
                (int)MathF.Round(normal.Y),
                (int)MathF.Round(normal.Z));

            if (IsWithinChunkBounds(adjacentPosition))
                return !Blocks.ContainsKey(adjacentPosition) || Blocks[adjacentPosition] == BlockList.AIR;
            else
            {
                Vector3i worldPosition = CoordinateHelper.BlockToWorldCoordinates(position, CoordinateHelper.WorldToChunkCoordinates(GameObject.Transform.WorldPosition));
                Vector3i adjacentWorldPosition = worldPosition + new Vector3i(
                    (int)MathF.Round(normal.X),
                    (int)MathF.Round(normal.Y),
                    (int)MathF.Round(normal.Z));

                Vector3i adjacentChunkCoord = CoordinateHelper.WorldToChunkCoordinates(adjacentWorldPosition);
                Vector3i adjacentBlockCoord = CoordinateHelper.WorldToBlockCoordinates(adjacentWorldPosition);

                if (world.GetLoadedChunks().TryGetValue(adjacentChunkCoord, out Chunk? adjacentChunk))
                    return !adjacentChunk.Blocks.ContainsKey(adjacentBlockCoord) || adjacentChunk.Blocks[adjacentBlockCoord] == BlockList.AIR;
                else
                    return true;
            }
        }

        private bool IsWithinChunkBounds(Vector3i position)
        {
            return position.X >= 0 && position.X < CHUNK_SIZE &&
                   position.Y >= 0 && position.Y < CHUNK_SIZE &&
                   position.Z >= 0 && position.Z < CHUNK_SIZE;
        }

        private void AddFace(Vector3i position, Vector3f normal, BlockData blockData, TextureAtlas atlas, int faceIndex)
        {
            uint startIndex = (uint)Vertices.Count;

            Vector3f[] faceVertices = GetFaceVertices(position, faceIndex);

            Vector3f color = GetFaceColor(normal, blockData);
            Vector2f[] uv = GetFaceUV(blockData, normal, atlas);

            Vertices.Add(new Vertex(faceVertices[0], color, normal, uv[0]));
            Vertices.Add(new Vertex(faceVertices[1], color, normal, uv[1]));
            Vertices.Add(new Vertex(faceVertices[2], color, normal, uv[2]));
            Vertices.Add(new Vertex(faceVertices[3], color, normal, uv[3]));

            Indices.Add(startIndex + 0);
            Indices.Add(startIndex + 1);
            Indices.Add(startIndex + 2);
            Indices.Add(startIndex + 0);
            Indices.Add(startIndex + 2);
            Indices.Add(startIndex + 3);
        }

        private Vector3f GetFaceColor(Vector3f normal, BlockData blockData)
        {
            if (normal == new Vector3f(0, 1, 0))
                return blockData.TopColor;
            else if (normal == new Vector3f(0, -1, 0))
                return Vector3f.One;
            else
                return Vector3f.One;
        }

        private Vector2f[] GetFaceUV(BlockData blockData, Vector3f normal, TextureAtlas atlas)
        {
            if (normal == new Vector3f(0, 1, 0))
                return atlas.GetTextureCoordinates(blockData.Textures["top"]);
            else if (normal == new Vector3f(0, -1, 0))
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
                _ => throw new ArgumentOutOfRangeException(),
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
            Chunk result = new();

            result.Initialize(generateNothing);

            return result;
        }
    }
}
