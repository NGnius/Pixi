using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

using RobocraftX.Common;
using Newtonsoft.Json;
using Unity.Mathematics;

using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Utility;

namespace Pixi.Robots
{
    public static class CubeUtility
    {
		private static Dictionary<uint, string> map = null;

		public static RobotStruct? ParseRobotInfo(string robotInfo)
		{
			try
			{
				return JsonConvert.DeserializeObject<RobotStruct>(robotInfo);
			}
			catch (Exception e)
			{
				Logging.MetaLog(e);
				return null;
			}
		}

		public static CubeInfo[] ParseCubes(RobotStruct robot)
		{
			return ParseCubes(robot.cubeData, robot.colourData);
		}

		public static CubeInfo[] ParseCubes(string cubeData, string colourData)
		{
			BinaryBufferReader cubes = new BinaryBufferReader(Convert.FromBase64String(cubeData), 0);
			BinaryBufferReader colours = new BinaryBufferReader(Convert.FromBase64String(colourData), 0);
			uint cubeCount = cubes.ReadUint();
			uint colourCount = colours.ReadUint();
			if (cubeCount != colourCount)
			{
				Logging.MetaLog("Something is fucking broken");
				return null;
			}
			Logging.MetaLog($"Detected {cubeCount} cubes");
			CubeInfo[] result = new CubeInfo[cubeCount];
			for (int cube = 0; cube < cubeCount; cube++)
			{
				result[cube] = TranslateSpacialEnumerations(
					cubes.ReadUint(),
					cubes.ReadByte(),
					cubes.ReadByte(),
					cubes.ReadByte(),
					cubes.ReadByte(),
					colours.ReadByte(),
					colours.ReadByte(),
					colours.ReadByte(),
					colours.ReadByte()
				);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CubeInfo TranslateSpacialEnumerations(uint cubeId, byte x, byte y, byte z, byte rotation, byte colour, byte colour_x, byte colour_y, byte colour_z)
		{
			if (x != colour_x || z != colour_z || y != colour_y) return default;
			CubeInfo result = new CubeInfo { visible = true };
			TranslateBlockColour(colour, ref result);
			TranslateBlockPosition(x, y, z, ref result);
			TranslateBlockRotation(rotation, ref result);
			TranslateBlockId(cubeId, ref result);
#if DEBUG
			Logging.MetaLog($"Cube {cubeId} ({x}, {y}, {z}) rot:{rotation} decoded as {result.block} {result.position} rot: {result.rotation} color: {result.color} {result.darkness}");
#endif
			return result;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TranslateBlockRotation(byte rotation, ref CubeInfo result)
		{
            // face refers to the face of the block connected to the bottom of the current one
            // nvm, they're all incorrect
			switch (rotation)
			{
				case 0:
					result.rotation = new float3(0, 0, 0); // top face, forwards
					break;
				case 1:
					result.rotation = new float3(0, 0, 90); // left face, forwards
					break;
				case 2:
                    result.rotation = new float3(0, 0, 180); // bottom face, forwards
                    break;
				case 3:
                    result.rotation = new float3(0, 0, -90); // front face, down
                    break;
				case 4:
                    result.rotation = new float3(0, 90, 0); // top face, right
                    break;
				case 5:
                    result.rotation = new float3(0, 90, 90); // front face, right
                    break;
				case 6:
                    result.rotation = new float3(-90, -90, 0); // right face, backwards
                    break;
				case 7:
                    result.rotation = new float3(0, 90, -90); // back face, right
                    break;
				case 8:
                    result.rotation = new float3(0, -90, 90); // back face, left
                    break;
				case 9:
                    result.rotation = new float3(0, -90, -90); // front face, left
                    break;
				case 10:
                    result.rotation = new float3(90, -90, 0); // left face, down
                    break;
				case 11:
                    result.rotation = new float3(90, 90, 0); // right face, forwards
                    break;
				case 12:
                    result.rotation = new float3(-90, 90, 0); // left face, up
                    break;
				case 13:
                    result.rotation = new float3(0, 90, 180); // bottom face, right
                    break;
				case 14:
                    result.rotation = new float3(0, 180, 0); // top face, backwards
                    break;
				case 15:
                    result.rotation = new float3(0, 180, 90); // right face, up
                    break;
				case 16:
                    result.rotation = new float3(0, 180, 180); // bottom face, backwards
                    break;
				case 17:
                    result.rotation = new float3(0, 180, -90); // left face, backwards
                    break;
				case 18:
                    result.rotation = new float3(0, -90, 0); // top face, left
                    break;
				case 19:
                    result.rotation = new float3(0, -90, 180); // bottom face, left
                    break;
				case 20:
                    result.rotation = new float3(90, 0, 0); // front face, down
                    break;
				case 21:
                    result.rotation = new float3(90, 180, 0); // back face, down
                    break;
				case 22:
                    result.rotation = new float3(-90, 0, 0); // back face, up
                    break;
				case 23:
                    result.rotation = new float3(-90, 180, 0); // front face, up
                    break;
				default:
#if DEBUG
					Logging.MetaLog($"Unknown rotation {rotation.ToString("X2")}");
#endif
					result.rotation = float3.zero;
					break;
			}
            // my brain hurts after figuring out all of those rotations
            // I wouldn't recommend trying to redo this
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TranslateBlockPosition(byte x, byte y, byte z, ref CubeInfo result)
		{
			// for some reason, z is forwards in garage bays
			result.position = new float3(x, y, z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TranslateBlockColour(byte colour, ref CubeInfo result)
		{
            // I hope these colours are accurate, I just guessed
            // TODO colour accuracy (lol that won't ever happen)
			switch (colour)
			{
				case 0:
					result.color = BlockColors.White;
					result.darkness = 0;
					break;
				case 1:
					result.color = BlockColors.White;
					result.darkness = 5;
					break;
				case 2:
					result.color = BlockColors.Orange;
                    result.darkness = 0;
					break;
                case 3:
					result.color = BlockColors.Blue;
                    result.darkness = 2;
					break;
				case 4:
                    result.color = BlockColors.White;
                    result.darkness = 8;
					break;
                case 5:
					result.color = BlockColors.Red;
                    result.darkness = 0;
					break;
                case 6:
					result.color = BlockColors.Yellow;
                    result.darkness = 0;
					break;
                case 7:
					result.color = BlockColors.Green;
                    result.darkness = 0;
					break;
				case 8:
					result.color = BlockColors.Purple;
                    result.darkness = 0;
					break;
                case 9:
                    result.color = BlockColors.Blue;
                    result.darkness = 7;
					break;
                case 10:
                    result.color = BlockColors.Purple;
                    result.darkness = 5;
					break;
                case 11:
                    result.color = BlockColors.Orange;
                    result.darkness = 7;
					break;
				case 12:
					result.color = BlockColors.Green;
                    result.darkness = 3;
					break;
                case 13:
					result.color = BlockColors.Green;
                    result.darkness = 2;
					break;
                case 14:
					result.color = BlockColors.Pink;
                    result.darkness = 3;
					break;
                case 15:
                    result.color = BlockColors.Pink;
                    result.darkness = 2;
					break;
				case 16:
                    result.color = BlockColors.Red;
                    result.darkness = 2;
					break;
                case 17:
                    result.color = BlockColors.Orange;
                    result.darkness = 8;
					break;
                case 18:
                    result.color = BlockColors.Red;
                    result.darkness = 7;
					break;
                case 19:
                    result.color = BlockColors.Pink;
                    result.darkness = 0;
					break;
				case 20:
					result.color = BlockColors.Yellow;
                    result.darkness = 2;
					break;
                case 21:
					result.color = BlockColors.Green;
                    result.darkness = 7;
					break;
                case 22:
					result.color = BlockColors.Green;
                    result.darkness = 8;
					break;
                case 23:
                    result.color = BlockColors.Blue;
                    result.darkness = 8;
					break;
				case 24:
					result.color = BlockColors.Aqua;
                    result.darkness = 7;
					break;
                case 25:
					result.color = BlockColors.Blue;
                    result.darkness = 6;
					break;
                case 26:
					result.color = BlockColors.Aqua;
                    result.darkness = 5;
					break;
                case 27:
                    result.color = BlockColors.Blue;
                    result.darkness = 4;
					break;
				case 28:
					result.color = BlockColors.Aqua;
                    result.darkness = 3;
					break;
                case 29:
					result.color = BlockColors.Blue;
                    result.darkness = 5;
					break;
                case 30:
					result.color = BlockColors.Purple;
                    result.darkness = 3;
					break;
                case 31:
					result.color = BlockColors.Purple;
                    result.darkness = 1;
					break;
				default:
					result.color = BlockColors.Aqua;
					result.darkness = 0;
					break;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TranslateBlockId(uint cubeId, ref CubeInfo result)
		{
            if (map == null)
			{
				StreamReader cubemap = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Pixi.cubes-id.json"));
                map = JsonConvert.DeserializeObject<Dictionary<uint, string>>(cubemap.ReadToEnd());
			}

			if (!map.ContainsKey(cubeId))
			{
				result.block = BlockIDs.TextBlock;
				result.name = "Unknown cube #" + cubeId.ToString();
				//result.rotation = float3.zero;
#if DEBUG
				Logging.MetaLog($"Unknown cubeId {cubeId}");
#endif
			}
			string cubeName = map[cubeId];
			if (cubeName.Contains("cube"))
			{
				result.block = BlockIDs.AluminiumCube;
                result.rotation = float3.zero;
			}
			else if (cubeName.Contains("prism") || cubeName.Contains("edge"))
			{
				if (cubeName.Contains("round"))
				{
					if (cubeName.Contains("glass") || cubeName.Contains("windshield"))
                    {
                        result.block = BlockIDs.GlassRoundedSlope;
                    } else
					    result.block = BlockIDs.AluminiumRoundedSlope;
				}
				else
				{
                    if (cubeName.Contains("glass") || cubeName.Contains("windshield"))
                    {
						result.block = BlockIDs.GlassSlope;
                    } else
					    result.block = BlockIDs.AluminiumSlope;
				}
			}
			else if (cubeName.Contains("inner"))
            {
                if (cubeName.Contains("round"))
                {
					if (cubeName.Contains("glass") || cubeName.Contains("windshield"))
					{
						result.block = BlockIDs.GlassRoundedSlicedCube;
					} else
					    result.block = BlockIDs.AluminiumRoundedSlicedCube;
                }
				else
				{
                    if (cubeName.Contains("glass") || cubeName.Contains("windshield"))
                    {
                        result.block = BlockIDs.GlassSlicedCube;
                    } else
					    result.block = BlockIDs.AluminiumSlicedCube;
				}
			}
			else if (cubeName.Contains("tetra") || cubeName.Contains("corner"))
            {
                if (cubeName.Contains("round"))
                {
                    if (cubeName.Contains("glass") || cubeName.Contains("windshield"))
                    {
						result.block = BlockIDs.GlassRoundedCorner;
                    } else
                        result.block = BlockIDs.AluminiumRoundedCorner;
                }
				else
				{
                    if (cubeName.Contains("glass") || cubeName.Contains("windshield"))
                    {
                        result.block = BlockIDs.GlassCorner;
                    } else
					    result.block = BlockIDs.AluminiumCorner;
				}
			}
			else if (cubeName.Contains("pyramid"))
			{
				result.block = BlockIDs.AluminiumPyramidSegment;
			}
			else if (cubeName.Contains("cone"))
			{
				result.block = BlockIDs.AluminiumConeSegment;
			}
			else
			{
				result.block = BlockIDs.TextBlock;
				result.name = cubeName;
			}
		}
	}
}
