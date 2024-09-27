using Invasion.Math;
using System.Collections.Generic;

namespace Invasion.Block
{
    public struct BlockData(string registryName, string displayName, short id, bool isSolid, Vector3f color, Dictionary<string, string> textures)
    {
        public string RegistryName { get; set; } = registryName;
        public string DisplayName { get; set; } = displayName;
        public short ID { get; set; } = id;
        public bool IsSolid { get; set; } = isSolid;
        public Vector3f TopColor { get; set; } = color;

        public Dictionary<string, string> Textures { get; set; } = textures;
    }

    public static class BlockList
    {
        public const short AIR = 0;
        public const short GRASS = 1;
        public const short DIRT = 2;
        public const short STONE = 3;
        public const short SAND = 4;
        public const short WATER = 5;
        public const short WOOD = 6;
        public const short LEAVES = 7;
        public const short GLASS = 8;
        public const short COBBLESTONE = 9;
        public const short BRICKS = 10;
        public const short PLANKS = 11;
        public const short BEDROCK = 12;
        public const short DIAMOND_ORE = 13;
        public const short DIAMOND_BLOCK = 14;
        public const short GOLD_ORE = 15;
        public const short GOLD_BLOCK = 16;
        public const short IRON_ORE = 17;
        public const short IRON_BLOCK = 18;
        public const short COAL_ORE = 19;
        public const short COAL_BLOCK = 20;
        public const short LAPIS_ORE = 21;
        public const short LAPIS_BLOCK = 22;
        public const short REDSTONE_ORE = 23;
        public const short REDSTONE_BLOCK = 24;
        public const short EMERALD_ORE = 25;
        public const short EMERALD_BLOCK = 26;
        public const short OBSIDIAN = 27;
        public const short TNT = 28;
        public const short NETHERRACK = 29;
        public const short SOUL_SAND = 30;
        public const short NETHER_BRICKS = 31;
        public const short GLOWSTONE = 32;
        public const short END_STONE = 33;

        public static BlockData GetBlockData(short block) => block switch
        {
            AIR => new BlockData("block_air", "Air", AIR, false, Vector3f.One, []),
            GRASS => new BlockData("block_grass", "Grass", GRASS, true, new(0.0f, 0.75f, 0.55f), new Dictionary<string, string>
            {
                { "top", "grass_top" },
                { "side", "grass_side" },
                { "bottom", "dirt" }
            }),
            DIRT => new BlockData("block_dirt", "Dirt", DIRT, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "dirt" },
                { "side", "dirt" },
                { "bottom", "dirt" }
            }),
            STONE => new BlockData("block_stone", "Stone", STONE, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "stone" },
                { "side", "stone" },
                { "bottom", "stone" }
            }),
            SAND => new BlockData("block_sand", "Sand", SAND, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "sand" },
                { "side", "sand" },
                { "bottom", "sand" }
            }),
            WATER => new BlockData("block_water", "Water", WATER, false, Vector3f.One, []),
            WOOD => new BlockData("block_wood", "Wood", WOOD, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "wood_top" },
                { "side", "wood_side" },
                { "bottom", "wood_top" }
            }),
            LEAVES => new BlockData("block_leaves", "Leaves", LEAVES, false, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "leaves_oak" },
                { "side", "leaves_oak" },
                { "bottom", "leaves_oak" }
            }),
            GLASS => new BlockData("block_glass", "Glass", GLASS, false, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "glass" },
                { "side", "glass" },
                { "bottom", "glass" }
            }),
            COBBLESTONE => new BlockData("block_cobblestone", "Cobblestone", COBBLESTONE, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "cobblestone" },
                { "side", "cobblestone" },
                { "bottom", "cobblestone" }
            }),
            BRICKS => new BlockData("block_bricks", "Bricks", BRICKS, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "bricks" },
                { "side", "bricks" },
                { "bottom", "bricks" }
            }),
            PLANKS => new BlockData("block_planks", "Planks", PLANKS, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "planks_oak" },
                { "side", "planks_oak" },
                { "bottom", "planks_oak" }
            }),
            BEDROCK => new BlockData("block_bedrock", "Bedrock", BEDROCK, true, Vector3f.One, new Dictionary<string, string>
            {
                { "top", "bedrock" },
                { "side", "bedrock" },
                { "bottom", "bedrock" }
            }),
            _ => throw new System.Exception("Invalid block ID")
        };
    }
}