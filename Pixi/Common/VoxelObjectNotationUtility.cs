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
			BlockJsonInfo jsonInfo = new BlockJsonInfo
			{
				name = block.Type.ToString(),
				position = new float[3] { block.Position.x - origin[0], block.Position.y - origin[1], block.Position.z - origin[2]},
				rotation = new float[3] { block.Rotation.x, block.Rotation.y, block.Rotation.z },
				color = ColorSpaceUtility.UnquantizeToArray(block.Color),
				scale = new float[3] {block.Scale.x, block.Scale.y, block.Scale.z},
			};
			// custom stats for special blocks
			switch (block.Type)
			{
				case BlockIDs.TextBlock:
					TextBlock t = block.Specialise<TextBlock>();
					jsonInfo.name += "\t" + t.Text + "\t" + t.TextBlockId;
					break;
				case BlockIDs.ConsoleBlock:
					ConsoleBlock c = block.Specialise<ConsoleBlock>();
					jsonInfo.name += "\t" + c.Command + "\t" + c.Arg1 + "\t" + c.Arg2 + "\t" + c.Arg3;
					break;
				case BlockIDs.DampedSpring:
					DampedSpring d = block.Specialise<DampedSpring>();
					jsonInfo.name += "\t" + d.Stiffness + "\t" + d.Damping;
					break;
				case BlockIDs.ServoAxle:
				case BlockIDs.ServoHinge:
				case BlockIDs.PneumaticAxle:
				case BlockIDs.PneumaticHinge:
					Servo s = block.Specialise<Servo>();
					jsonInfo.name += "\t" + s.MinimumAngle + "\t" + s.MaximumAngle + "\t" + s.MaximumForce + "\t" +
					                 s.Reverse;
					break;
				case BlockIDs.MotorM:
				case BlockIDs.MotorS:
					Motor m = block.Specialise<Motor>();
					jsonInfo.name += "\t" + m.TopSpeed + "\t" + m.Torque + "\t" + m.Reverse;
					break;
				default: break;
			}
			return jsonInfo;
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
