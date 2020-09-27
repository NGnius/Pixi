using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;

namespace Pixi.Common
{
    public static class VoxelObjectNotationUtility
    {
		private static readonly float[] origin_base = new float[3] { 0, 0, 0 };

		private static Dictionary<string, BlockIDs> enumMap = null;
        
		public static string SerializeBlocks(Block[] blocks, float[] origin = null)
		{
			BlockJsonInfo[] blockJsons = new BlockJsonInfo[blocks.Length];
			for (int i = 0; i < blocks.Length; i++)
			{
				blockJsons[i] = JsonObject(blocks[i], origin);
			}
			return JsonConvert.SerializeObject(blockJsons);
		}

		public static byte[] SerializeBlocksToBytes(Block[] blocks)
        {
			return Encoding.UTF8.GetBytes(SerializeBlocks(blocks));
        }

		public static BlockJsonInfo[] DeserializeBlocks(byte[] data)
		{
			return DeserializeBlocks(Encoding.UTF8.GetString(data));
		}

		public static BlockJsonInfo[] DeserializeBlocks(string data)
        {
            return JsonConvert.DeserializeObject<BlockJsonInfo[]>(data);
        }

		public static BlockJsonInfo JsonObject(Block block, float[] origin = null)
		{
			if (origin == null) origin = origin_base;
			return new BlockJsonInfo
			{
				name = block.Type == BlockIDs.TextBlock ? block.Label : block.Type.ToString(),
				position = new float[3] { block.Position.x - origin[0], block.Position.y - origin[1], block.Position.z - origin[2]},
				rotation = new float[3] { block.Rotation.x, block.Rotation.y, block.Rotation.z },
				color = ColorSpaceUtility.UnquantizeToArray(block.Color),
				scale = new float[3] {block.Scale.x, block.Scale.y, block.Scale.z},
			};
		}

		public static BlockIDs NameToEnum(BlockJsonInfo block)
		{
			return NameToEnum(block.name);
		}

		public static BlockIDs NameToEnum(string name)
		{
			if (enumMap == null) GenerateEnumMap();
			return enumMap[name];
		}

        private static void GenerateEnumMap()
		{
			enumMap = new Dictionary<string, BlockIDs>();
			foreach(BlockIDs e in Enum.GetValues(typeof(BlockIDs)))
			{
				enumMap[e.ToString()] = e;
			}
		}
    }
}
