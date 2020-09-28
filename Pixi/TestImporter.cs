using System;
using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Players;
using Pixi.Common;
using Unity.Mathematics;

namespace Pixi
{
    public class TestImporter : Importer
    {
        public int Priority { get; } = 0;
        public bool Optimisable { get; } = false;
        public string Name { get; } = "Test~Spell";
        public BlueprintProvider BlueprintProvider { get; } = null;
        public bool Qualifies(string name)
        {
            return name.Equals("test", StringComparison.InvariantCultureIgnoreCase);
        }

        public BlockJsonInfo[] Import(string name)
        {
            return new[]
            {
                new BlockJsonInfo
                {
                    name = BlockIDs.TextBlock.ToString() +
                           "\ttext that is preserved through the whole import process and ends up in the text block\ttextblockIDs_sux",
                    position = new[] {0f, 0f, 0f},
                    rotation = new[] {0f, 0f, 0f},
                    color = new[] {0f, 0f, 0f},
                    scale = new[] {1f, 1f, 1f},
                }
            };
        }

        public void PreProcess(string name, ref ProcessedVoxelObjectNotation[] blocks)
        {
            Player p = new Player(PlayerType.Local);
            float3 pos = p.Position;
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].position += pos;
            }
        }

        public void PostProcess(string name, ref Block[] blocks)
        {
            // meh
        }
    }
}