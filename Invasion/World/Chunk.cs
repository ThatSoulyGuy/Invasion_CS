using Invasion.Block;
using Invasion.ECS;
using Invasion.Render;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Invasion.World
{
    public class Chunk : Component
    {
        public const byte CHUNK_SIZE = 16;

        public short[,,] Blocks { get; private set; } = new short[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

        private List<Vertex> Vertices { get; set; } = [];
        private List<uint> Indices { get; set; } = [];

        private static Vector3[] FaceNormals { get; } =
        [
            new Vector3( 0,  0,  1),
            new Vector3( 0,  0, -1),
            new Vector3( 1,  0,  0),
            new Vector3(-1,  0,  0),
            new Vector3( 0,  1,  0),
            new Vector3( 0, -1,  0),
        ];

        private Chunk() { }

        private new void Initialize()
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

        public void Generate()
        {
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

                        for (int i = 0; i < FaceNormals.Length; i++)
                        {
                            Vector3 normal = FaceNormals[i];
                            Vector3 facePosition = new(x, y, z);

                            if (IsFaceExposed(facePosition, normal))
                            {
                                if (normal == new Vector3(0, 1, 0))
                                    AddFace(facePosition, normal, atlas.GetTextureCoordinates(BlockList.GetBlockData(block).Textures["top"]), i);
                                else if (normal == new Vector3(0, -1, 0))
                                    AddFace(facePosition, normal, atlas.GetTextureCoordinates(BlockList.GetBlockData(block).Textures["bottom"]), i);
                                else
                                    AddFace(facePosition, normal, atlas.GetTextureCoordinates(BlockList.GetBlockData(block).Textures["side"]), i);
                            }  
                        }
                    }
                }
            }

            Mesh mesh = GameObject.GetComponent<Mesh>();

            mesh.Vertices = Vertices;
            mesh.Indices = Indices;

            mesh.Generate();
        }

        private bool IsFaceExposed(Vector3 position, Vector3 normal)
        {
            Vector3 adjacentPosition = position + normal;

            int adjacentX = (int)adjacentPosition.X;
            int adjacentY = (int)adjacentPosition.Y;
            int adjacentZ = (int)adjacentPosition.Z;

            if (adjacentX < 0 || adjacentX >= CHUNK_SIZE || adjacentY < 0 || adjacentY >= CHUNK_SIZE || adjacentZ < 0 || adjacentZ >= CHUNK_SIZE)
                return true;
            
            return Blocks[adjacentX, adjacentY, adjacentZ] == 0;
        }

        public void AddFace(Vector3 position, Vector3 normal, Vector2[] uv, int faceIndex)
        {
            uint startIndex = (uint)Vertices.Count;

            Vector3[] faceVertices = GetFaceVertices(position, faceIndex);

            Vertices.Add(new Vertex(faceVertices[0], Vector3.One, normal, uv[0]));
            Vertices.Add(new Vertex(faceVertices[1], Vector3.One, normal, uv[1]));
            Vertices.Add(new Vertex(faceVertices[2], Vector3.One, normal, uv[2]));
            Vertices.Add(new Vertex(faceVertices[3], Vector3.One, normal, uv[3]));

            Indices.Add(startIndex + 0);
            Indices.Add(startIndex + 1);
            Indices.Add(startIndex + 2);
            Indices.Add(startIndex + 0);
            Indices.Add(startIndex + 2);
            Indices.Add(startIndex + 3);
        }

        private Vector3[] GetFaceVertices(Vector3 blockPosition, int faceIndex)
        {
            return faceIndex switch
            {
                0 => 
                [
                    blockPosition + new Vector3(0, 0, 1),
                    blockPosition + new Vector3(1, 0, 1),
                    blockPosition + new Vector3(1, 1, 1),
                    blockPosition + new Vector3(0, 1, 1),
                ],

                1 => 
                [
                    blockPosition + new Vector3(1, 0, 0),
                    blockPosition + new Vector3(0, 0, 0),
                    blockPosition + new Vector3(0, 1, 0),
                    blockPosition + new Vector3(1, 1, 0),
                ],
                
                2 => 
                [
                    blockPosition + new Vector3(1, 0, 1),
                    blockPosition + new Vector3(1, 0, 0),
                    blockPosition + new Vector3(1, 1, 0),
                    blockPosition + new Vector3(1, 1, 1),
                ],
                
                3 => 
                [
                    blockPosition + new Vector3(0, 0, 0),
                    blockPosition + new Vector3(0, 0, 1),
                    blockPosition + new Vector3(0, 1, 1),
                    blockPosition + new Vector3(0, 1, 0),
                ],
                
                4 => 
                [
                    blockPosition + new Vector3(0, 1, 1),
                    blockPosition + new Vector3(1, 1, 1),
                    blockPosition + new Vector3(1, 1, 0),
                    blockPosition + new Vector3(0, 1, 0),
                ],
                
                5 => 
                [
                    blockPosition + new Vector3(0, 0, 0),
                    blockPosition + new Vector3(1, 0, 0),
                    blockPosition + new Vector3(1, 0, 1),
                    blockPosition + new Vector3(0, 0, 1),
                ],

                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static Chunk Create()
        {
            Chunk result = new();

            result.Initialize();

            return result;
        }
    }
}
