using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;

namespace Pixi.Common
{
    public static class BlueprintUtility
    {
        public static Dictionary<string, BlockJsonInfo[]> ParseBlueprintFile(string name)
        {
            StreamReader bluemap = new StreamReader(File.OpenRead(name));
            return JsonConvert.DeserializeObject<Dictionary<string, BlockJsonInfo[]>>(bluemap.ReadToEnd());
        }
        
        public static Dictionary<string, BlockJsonInfo[]> ParseBlueprintResource(string name)
        {
            StreamReader bluemap = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name));
            return JsonConvert.DeserializeObject<Dictionary<string, BlockJsonInfo[]>>(bluemap.ReadToEnd());
        }

        public static ProcessedVoxelObjectNotation[][] ProcessAndExpandBlocks(string name, BlockJsonInfo[] blocks, BlueprintProvider blueprints)
        {
            List<ProcessedVoxelObjectNotation[]> expandedBlocks = new List<ProcessedVoxelObjectNotation[]>();
            for (int i = 0; i < blocks.Length; i++)
            {
                ProcessedVoxelObjectNotation root = blocks[i].Process();
                if (root.blueprint)
                {
                    if (blueprints == null)
                    {
                        throw new NullReferenceException("Blueprint block info found but BlueprintProvider is null");
                    }

                    BlockJsonInfo[] blueprint = blueprints.Blueprint(name, blocks[i]);
                    ProcessedVoxelObjectNotation[] expanded = new ProcessedVoxelObjectNotation[blueprint.Length];
                    for (int j = 0; j < expanded.Length; j++)
                    {
                        expanded[j] = blueprint[j].Process();
                    }

                    expandedBlocks.Add(expanded);
                }
                else
                {
                    expandedBlocks.Add(new ProcessedVoxelObjectNotation[]{root});
                }
            }
            return expandedBlocks.ToArray();
        }

        public static ProcessedVoxelObjectNotation[] ProcessBlocks(BlockJsonInfo[] blocks)
        {
            ProcessedVoxelObjectNotation[] procBlocks = new ProcessedVoxelObjectNotation[blocks.Length];
            for (int i = 0; i < blocks.Length; i++)
            {
                procBlocks[i] = blocks[i].Process();
            }

            return procBlocks;
        }
    }
}