using Invasion.ECS;
using Invasion.Math;
using Invasion.Render;
using Invasion.Util;
using System.Collections.Generic;
using System.Linq;


namespace Invasion.World
{
    public class IWorld : Component
    {
        public string RegistryName { get; init; } = string.Empty;

        public const byte LOADER_DISTANCE = 3;

        public Vector3f[] LoaderPositions { get; set; } = [];

        private Dictionary<Vector3i, Chunk> LoadedChunks { get; } = [] ;
        private List<Vector3i> ChunksToBeLoaded { get; set; } = [];
        private List<Vector3i> ChunksToBeUnloaded { get; set; } = [];

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
                            ChunksToBeLoaded.Add(chunkPos);
                    }
                }
            }

            ChunksToBeUnloaded = LoadedChunks.Keys.Where(chunkPos => !requiredChunks.Contains(chunkPos)).ToList();
        }

        public void LoadReadyChunks()
        {
            foreach (var chunkPos in ChunksToBeLoaded)
                GenerateChunk(CoordinateHelper.ChunkToWorldCoordinates(chunkPos));
        }

        public void UnloadReadyChunks()
        {
            foreach (var chunkPos in ChunksToBeUnloaded)
                UnloadChunk(CoordinateHelper.ChunkToWorldCoordinates(chunkPos));
        }

        public Chunk GenerateChunk(Vector3i position)
        {
            if (GameObjectManager.Get($"Chunk_Object_{position.X}_{position.Y}_{position.Z}_") != null)
                return LoadedChunks[CoordinateHelper.WorldToChunkCoordinates(position)];

            GameObject chunkObject = GameObject.Create($"Chunk_Object_{position.X}_{position.Y}_{position.Z}_");
            
            chunkObject.Transform.LocalPosition = position;

            chunkObject.AddComponent(ShaderManager.Get("default"));
            chunkObject.AddComponent(TextureAtlasManager.Get("blocks").Atlas);
            chunkObject.AddComponent(TextureAtlasManager.Get("blocks"));

            chunkObject.AddComponent(Mesh.Create($"Chunk_Mesh_{position.X}_{position.Y}_{position.Z}_", [], []));

            Chunk result = chunkObject.AddComponent(Chunk.Create());

            result.Generate();

            LoadedChunks.Add(CoordinateHelper.WorldToChunkCoordinates(position), result);

            return result;
        }

        public void UnloadChunk(Vector3i position)
        {
            if (LoadedChunks.ContainsKey(CoordinateHelper.WorldToChunkCoordinates(position)))
            {
                GameObjectManager.Unregister(LoadedChunks[CoordinateHelper.WorldToChunkCoordinates(position)].GameObject.Name);

                LoadedChunks.Remove(CoordinateHelper.WorldToChunkCoordinates(position));
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