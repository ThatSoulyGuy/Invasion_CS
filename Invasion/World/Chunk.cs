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

        public short[,,] Blocks { get; private set; } = new short[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

        private List<Vertex> Vertices { get; set; } = [];
        private List<uint> Indices { get; set; } = [];

        private Dictionary<Vector3i, BoundingBox> Colliders { get; } = [];

        private static Vector3f[] FaceNormals { get; } =
        [
            new( 0,  0,  1),
            new( 0,  0, -1),
            new( 1,  0,  0),
            new(-1,  0,  0),
            new( 0,  1,  0),
            new( 0, -1,  0),
        ];

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
                            if (y > 11)
                                Blocks[x, y, z] = BlockList.DIRT;
                            else
                                Blocks[x, y, z] = BlockList.STONE;

                            Blocks[x, 15, z] = BlockList.GRASS;
                            Blocks[x, 0, z] = BlockList.BEDROCK;
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

            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        short block = Blocks[x, y, z];

                        if (block == BlockList.AIR)
                            continue;

                        Vector3i position = new(x, y, z);

                        for (int i = 0; i < FaceNormals.Length; i++)
                        {
                            Vector3f normal = FaceNormals[i];
                            Vector3i facePosition = new(x, y, z);

                            if (IsFaceExposed(facePosition, normal))
                            {
                                if (!Colliders.ContainsKey(position))
                                {
                                    Vector3f worldPosition = CoordinateHelper.BlockToWorldCoordinates(position, CoordinateHelper.WorldToChunkCoordinates(GameObject.Transform.WorldPosition));
                                    worldPosition += new Vector3f(0.5f, 0.5f, 0.5f);

                                    Colliders.Add(position, BoundingBox.Create(worldPosition, Vector3f.One));
                                }

                                if (normal == new Vector3f(0, 1, 0))
                                    AddFace(facePosition, normal, BlockList.GetBlockData(block).TopColor, atlas.GetTextureCoordinates(BlockList.GetBlockData(block).Textures["top"]), i);
                                else if (normal == new Vector3f(0, -1, 0))
                                    AddFace(facePosition, normal, Vector3f.One, atlas.GetTextureCoordinates(BlockList.GetBlockData(block).Textures["bottom"]), i);
                                else
                                    AddFace(facePosition, normal, Vector3f.One, atlas.GetTextureCoordinates(BlockList.GetBlockData(block).Textures["side"]), i);
                            }  
                        }
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
            if (position.X < 0 || position.X >= CHUNK_SIZE || position.Y < 0 || position.Y >= CHUNK_SIZE || position.Z < 0 || position.Z >= CHUNK_SIZE)
                return;

            if (Blocks[position.X, position.Y, position.Z] == block)
                return;

            Blocks[position.X, position.Y, position.Z] = block;

            Generate();
        }

        private bool IsFaceExposed(Vector3i position, Vector3f normal)
        {
            Vector3f adjacentPosition = (Vector3f)position + normal;

            int adjacentX = (int)adjacentPosition.X;
            int adjacentY = (int)adjacentPosition.Y;
            int adjacentZ = (int)adjacentPosition.Z;

            if (adjacentX < 0 || adjacentX >= CHUNK_SIZE || adjacentY < 0 || adjacentY >= CHUNK_SIZE || adjacentZ < 0 || adjacentZ >= CHUNK_SIZE)
                return true;
            
            return Blocks[adjacentX, adjacentY, adjacentZ] == 0;
        }

        private void AddFace(Vector3f position, Vector3f normal, Vector3f color, Vector2f[] uv, int faceIndex)
        {
            uint startIndex = (uint)Vertices.Count;

            Vector3f[] faceVertices = GetFaceVertices(position, faceIndex);

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

        private Vector3f[] GetFaceVertices(Vector3f blockPosition, int faceIndex)
        {
            return faceIndex switch
            {
                0 => 
                [
                    blockPosition + new Vector3f(0, 0, 1),
                    blockPosition + new Vector3f(1, 0, 1),
                    blockPosition + new Vector3f(1, 1, 1),
                    blockPosition + new Vector3f(0, 1, 1),
                ],

                1 => 
                [
                    blockPosition + new Vector3f(1, 0, 0),
                    blockPosition + new Vector3f(0, 0, 0),
                    blockPosition + new Vector3f(0, 1, 0),
                    blockPosition + new Vector3f(1, 1, 0),
                ],
                
                2 => 
                [
                    blockPosition + new Vector3f(1, 0, 1),
                    blockPosition + new Vector3f(1, 0, 0),
                    blockPosition + new Vector3f(1, 1, 0),
                    blockPosition + new Vector3f(1, 1, 1),
                ],
                
                3 => 
                [
                    blockPosition + new Vector3f(0, 0, 0),
                    blockPosition + new Vector3f(0, 0, 1),
                    blockPosition + new Vector3f(0, 1, 1),
                    blockPosition + new Vector3f(0, 1, 0),
                ],
                
                4 => 
                [
                    blockPosition + new Vector3f(0, 1, 1),
                    blockPosition + new Vector3f(1, 1, 1),
                    blockPosition + new Vector3f(1, 1, 0),
                    blockPosition + new Vector3f(0, 1, 0),
                ],
                
                5 => 
                [
                    blockPosition + new Vector3f(0, 0, 0),
                    blockPosition + new Vector3f(1, 0, 0),
                    blockPosition + new Vector3f(1, 0, 1),
                    blockPosition + new Vector3f(0, 0, 1),
                ],

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
