using Invasion.ECS;
using Invasion.Math;
using Invasion.Render;
using Invasion.Util;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Invasion.World
{
    public class IWorld : Component
    {
        public string RegistryName { get; init; } = string.Empty;

        public const byte LOADER_DISTANCE = 3;

        public Vector3f[] LoaderPositions { get; set; } = [];

        private ConcurrentDictionary<Vector3i, Chunk> LoadedChunks { get; } = [] ;
        private List<Vector3i> ChunksToBeLoaded { get; set; } = [];
        private List<Vector3i> ChunksToBeUnloaded { get; set; } = [];

        private bool chunksNeedGeneration = false;

        private IWorld() { }
        
        public override void Update()
        {
            ChunksToBeLoaded.Clear();
            ChunksToBeUnloaded.Clear();
            HashSet<Vector3i> requiredChunks = [];

            foreach (var loaderPosition in LoaderPositions)
            {
                Vector3i loaderChunkCoord = CoordinateHelper.WorldToChunkCoordinates(loaderPosition);
                loaderChunkCoord.Y = 0;

                for (int x = -LOADER_DISTANCE; x <= LOADER_DISTANCE; x++)
                {
                    for (int z = -LOADER_DISTANCE; z <= LOADER_DISTANCE; z++)
                    {
                        Vector3i chunkPos = loaderChunkCoord + new Vector3i(x, 0, z);

                        requiredChunks.Add(chunkPos);

                        if (!LoadedChunks.ContainsKey(chunkPos) && !ChunksToBeLoaded.Contains(chunkPos))
                        {
                            ChunksToBeLoaded.Add(chunkPos);
                            chunksNeedGeneration = true;
                        }
                    }
                }
            }

            ChunksToBeUnloaded = LoadedChunks.Keys.Where(chunkPos => !requiredChunks.Contains(chunkPos) && chunkPos.Y == 0).ToList();

            LoadReadyChunks();
            UnloadReadyChunks();

            if (chunksNeedGeneration)
            {
                Task.Run(GenerateChunks);
                chunksNeedGeneration = false;
            }
        }

        public void SetBlock(Vector3f worldPosition, short block, bool createChunkIfNotPresent = false)
        {
            Vector3i chunkPos = CoordinateHelper.WorldToChunkCoordinates(worldPosition);
            Vector3i blockPos = CoordinateHelper.WorldToBlockCoordinates(worldPosition);

            if (LoadedChunks.TryGetValue(chunkPos, out Chunk? value))
                value.SetBlock(blockPos, block);
            else if (createChunkIfNotPresent)
            {
                GenerateChunk(CoordinateHelper.ChunkToWorldCoordinates(chunkPos), true, true);

                LoadedChunks[chunkPos].SetBlock(blockPos, block);
            }
        }

        public ConcurrentDictionary<Vector3i, Chunk> GetLoadedChunks()
        {
            return LoadedChunks;
        }

        public void GenerateChunks()
        {
            foreach (var chunk in LoadedChunks.Values)
            {
                var chunkPos = CoordinateHelper.WorldToChunkCoordinates(chunk.GameObject.Transform.WorldPosition);

                chunk.Generate();
            }
        }

        public void LoadReadyChunks()
        {
            foreach (var chunkPos in ChunksToBeLoaded)
                GenerateChunk(CoordinateHelper.ChunkToWorldCoordinates(chunkPos), false);
        }

        public void UnloadReadyChunks()
        {
            foreach (var chunkPos in ChunksToBeUnloaded)
                UnloadChunk(CoordinateHelper.ChunkToWorldCoordinates(chunkPos));
        }

        public Chunk GenerateChunk(Vector3i position, bool automaticallyGenerate = true, bool generateNothing = false)
        {
            if (GameObjectManager.Get($"Chunk_Object_{position.X}_{position.Y}_{position.Z}_") != null)
                return LoadedChunks[CoordinateHelper.WorldToChunkCoordinates(position)];

            GameObject chunkObject = GameObject.Create($"Chunk_Object_{position.X}_{position.Y}_{position.Z}_");
            
            chunkObject.Transform.LocalPosition = position;

            chunkObject.AddComponent(ShaderManager.Get("default"));
            chunkObject.AddComponent(TextureAtlasManager.Get("blocks").Atlas);
            chunkObject.AddComponent(TextureAtlasManager.Get("blocks"));

            chunkObject.AddComponent(Mesh.Create($"Chunk_Mesh_{position.X}_{position.Y}_{position.Z}_", [], []));

            Chunk result = chunkObject.AddComponent(Chunk.Create(generateNothing));

            if (automaticallyGenerate)
                result.Generate();

            LoadedChunks.TryAdd(CoordinateHelper.WorldToChunkCoordinates(position), result);

            return result;
        }

        public void UnloadChunk(Vector3i position)
        {
            Vector3i chunkPosition = CoordinateHelper.WorldToChunkCoordinates(position);

            if (LoadedChunks.TryGetValue(chunkPosition, out Chunk? value))
            {
                GameObjectManager.Unregister(value.GameObject.Name);

                LoadedChunks.TryRemove(chunkPosition, out _);
            }
        }

        public static IWorld Create(string registryName)
        {
            return new()
            {
                RegistryName = registryName
            };
        }
    }
}