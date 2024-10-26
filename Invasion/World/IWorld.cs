using Invasion.ECS;
using Invasion.Entity;
using Invasion.Math;
using Invasion.Render;
using Invasion.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Invasion.World
{
    public class IWorld : Component
    {
        public string RegistryName { get; init; } = string.Empty;

        public const byte LOADER_DISTANCE = 3;

        public Vector3f[] LoaderPositions { get; set; } = [];

        public const ulong TICK_RATE = 1000;
        
        public int CurrentUpdateCycle { get; set; } = 0;

        private ConcurrentDictionary<Vector3i, Chunk> LoadedChunks { get; } = [];

        private List<SpawnManager> SpawnManagers { get; set; } = [];

        private List<IEntity> Entities { get; } = [];

        private bool KeepUpdating = true;

        private static int MaxThreads { get; } = Environment.ProcessorCount / 2;
        private static BlockingCollection<Action> TaskQueue { get; } = [];
        private static List<Thread> ThreadPool { get; } = [];
        private object Lock { get; } = new();

        private IWorld()
        {
            lock (Lock)
            {
                for (int i = 0; i < MaxThreads; i++)
                {
                    Thread worker = new(() =>
                    {
                        foreach (var action in TaskQueue.GetConsumingEnumerable())
                        {
                            try
                            {
                                action();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Exception in thread pool {GetType()}: {ex.Message}");
                            }
                        }
                    })
                    {
                        IsBackground = true
                    };

                    worker.Start();
                    ThreadPool.Add(worker);
                }
            }
        }

        public override void Update()
        {
            lock (Lock)
            {
                CurrentUpdateCycle++;

                HashSet<Vector3i> requiredChunks = [];
                List<Vector3i> chunksToBeUnloaded = [];

                if (Time.Ticks % TICK_RATE == 0)
                {
                    foreach (var spawnManager in SpawnManagers)
                        spawnManager.OnSpawnTick(this, [.. LoadedChunks.Values]);
                }

                if (KeepUpdating)
                {
                    foreach (var loaderPosition in LoaderPositions)
                    {
                        Vector3i loaderChunkCoord = CoordinateHelper.WorldToChunkCoordinates(loaderPosition);
                        loaderChunkCoord.Y = 0;

                        for (int x = -LOADER_DISTANCE; x <= LOADER_DISTANCE; x++)
                        {
                            for (int z = -LOADER_DISTANCE; z <= LOADER_DISTANCE; z++)
                            {
                                Vector3i chunkPos = loaderChunkCoord + new Vector3i(x, 0, z);

                                if (!LoadedChunks.ContainsKey(chunkPos) && !requiredChunks.Contains(chunkPos))
                                    GenerateChunk(chunkPos);
                            
                                requiredChunks.Add(chunkPos);
                            }
                        }
                    }

                    chunksToBeUnloaded = LoadedChunks.Keys.Where(chunkPos => !requiredChunks.Contains(chunkPos) && chunkPos.Y == 0).ToList();

                    foreach (var chunkPos in chunksToBeUnloaded)
                        UnloadChunk(chunkPos);
                }

                foreach (var chunk in LoadedChunks.Values)
                {
                    if (chunk.NeedsRegeneration && chunk.LastUpdatedCycle < CurrentUpdateCycle)
                        chunk.Generate(CurrentUpdateCycle);
                }
                
                if (chunksToBeUnloaded.Count <= 0)
                    KeepUpdating = false;
            }
        }

        public Chunk GenerateChunk(Vector3i position, bool automaticallyGenerate = true, bool generateNothing = false)
        {
            if (LoadedChunks.TryGetValue(position, out Chunk? value))
                return value;

            GameObject chunkObject = GameObject.Create($"Chunk_Object_{position.X}_{position.Y}_{position.Z}_");

            chunkObject.Transform.LocalPosition = CoordinateHelper.ChunkToWorldCoordinates(position);

            chunkObject.AddComponent(ShaderManager.Get("default"));
            chunkObject.AddComponent(TextureAtlasManager.Get("blocks").Atlas);
            chunkObject.AddComponent(TextureAtlasManager.Get("blocks"));

            chunkObject.AddComponent(Mesh.Create($"Chunk_Mesh_{position.X}_{position.Y}_{position.Z}_", [], []));

            Chunk result = chunkObject.AddComponent(Chunk.Create(generateNothing));

            LoadedChunks.TryAdd(position, result);

            if (automaticallyGenerate)
                result.Generate(CurrentUpdateCycle);

            return result;
        }

        public void SetBlock(Vector3f worldPosition, short block, bool createChunkIfNotPresent = false)
        {
            Vector3i chunkPos = CoordinateHelper.WorldToChunkCoordinates(worldPosition);
            Vector3i blockPos = CoordinateHelper.WorldToBlockCoordinates(worldPosition);

            if (LoadedChunks.TryGetValue(chunkPos, out Chunk? value))
                value.SetBlock(blockPos, block);
            else if (createChunkIfNotPresent)
            {
                GenerateChunk(chunkPos, true, true);

                LoadedChunks[chunkPos].SetBlock(blockPos, block);
            }
        }

        public ConcurrentDictionary<Vector3i, Chunk> GetLoadedChunks()
        {
            return LoadedChunks;
        }

        public void AddSpawnManager(SpawnManager spawnManager)
        {
            SpawnManagers.Add(spawnManager);
        }

        public void RemoveSpawnManager(SpawnManager spawnManager)
        {
            SpawnManagers.Remove(spawnManager);
        }

        public GameObject SpawnEntity<T>(Vector3f position, bool hasRigidbody = true) where T : IEntity, new()
        {
            GameObject entityObject = GameObject.Create($"Entity_{typeof(T).Name}_{Entities.Count}");
            entityObject.Transform.LocalPosition = position;

            if (hasRigidbody)
                entityObject.AddComponent(Rigidbody.Create());

            entityObject.AddComponent(new T().ColliderSpecification);
            entityObject.AddComponent(new T());

            Entities.Add(entityObject.GetComponent<T>());

            return entityObject;
        }

        public GameObject SpawnEntity<T, A>(Vector3f position, bool hasRigidbody = true) where T : IEntity, new() where A : Component, new()
        {
            GameObject entityObject = GameObject.Create($"Entity_{typeof(T).Name}_{Entities.Count}");
            entityObject.Transform.LocalPosition = position;

            if (hasRigidbody)
                entityObject.AddComponent(Rigidbody.Create());

            entityObject.AddComponent(new A());
            entityObject.AddComponent(new T().ColliderSpecification);
            entityObject.AddComponent(new T());

            Entities.Add(entityObject.GetComponent<T>());

            return entityObject;
        }

        public GameObject SpawnEntity<T, A>(T classInstance, Vector3f position, bool hasRigidbody = true) where T : IEntity where A : Component, new()
        {
            GameObject entityObject = GameObject.Create($"Entity_{typeof(T).Name}_{Entities.Count}");
            entityObject.Transform.LocalPosition = position;

            if (hasRigidbody)
                entityObject.AddComponent(Rigidbody.Create());

            entityObject.AddComponent(new A());
            entityObject.AddComponent(classInstance.ColliderSpecification);
            entityObject.AddComponent(classInstance);

            Entities.Add(entityObject.GetComponent<T>());

            return entityObject;
        }

        public void KillEntity(IEntity entity)
        {
            GameObjectManager.Unregister(entity.GameObject.Name);
            Entities.Remove(entity);
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