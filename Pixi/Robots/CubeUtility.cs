using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

using RobocraftX.Common;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Utility;
using GamecraftModdingAPI;

using Pixi.Common;

namespace Pixi.Robots
{
    public static class CubeUtility
    {
		private static Dictionary<uint, string> map = null;

		private static Dictionary<uint, BlockJsonInfo[]> blueprintMap = null;

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
			CubeInfo result = new CubeInfo { visible = true, cubeId = cubeId };
			TranslateBlockColour(colour, ref result);
			TranslateBlockPosition(x, y, z, ref result);
			TranslateBlockRotation(rotation, ref result);
			TranslateBlockId(cubeId, ref result);
#if DEBUG
			Logging.MetaLog($"Cube {cubeId} ({x}, {y}, {z}) rot:{rotation} decoded as {result.block} {result.position} rot: {result.rotation} color: {result.color} {result.darkness}");
#endif
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
#if DEBUG
			Logging.MetaLog($"Cube colour {colour}");
#endif
			BlockColor c = ColorSpaceUtility.QuantizeToBlockColor(colour);
			result.color = c.Color;
			result.darkness = c.Darkness;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Block[] BuildBlueprintOrTextBlock(CubeInfo cube, float3 actualPosition, int scale = 3)
		{
            // actualPosition is the middle of the cube
			if (blueprintMap == null) LoadBlueprintMap();
			if (!blueprintMap.ContainsKey(cube.cubeId) || scale != 3)
			{
#if DEBUG
				Logging.LogWarning($"Missing blueprint for {cube.name} (id:{cube.cubeId}), substituting {cube.block}");
#endif
				return new Block[] { Block.PlaceNew(cube.block, actualPosition, cube.rotation, cube.color, cube.darkness, scale: cube.scale) };
			}
#if DEBUG
			Logging.MetaLog($"Found blueprint for {cube.name} (id:{cube.cubeId})");
#endif
			Quaternion cubeQuaternion = Quaternion.Euler(cube.rotation);
			BlockJsonInfo[] blueprint = blueprintMap[cube.cubeId];
			if (blueprint.Length == 0)
			{
				Logging.LogWarning($"Found empty blueprint for {cube.name} (id:{cube.cubeId}), is the blueprint correct?");
				return new Block[0];
			}
			float3 defaultCorrectionVec = new float3((float)(0), (float)(RobotCommands.blockSize), (float)(0));
			float3 baseRot = new float3(blueprint[0].rotation[0], blueprint[0].rotation[1], blueprint[0].rotation[2]);
			float3 baseScale = new float3(blueprint[0].scale[0], blueprint[0].scale[1], blueprint[0].scale[2]);
			Block[] placedBlocks = new Block[blueprint.Length];
			bool isBaseScaled = !(blueprint[0].scale[1] > 0f && blueprint[0].scale[1] < 2f);
			float3 correctionVec = isBaseScaled ? (float3)(Quaternion.Euler(baseRot) * baseScale / 2) * (float)-RobotCommands.blockSize : -defaultCorrectionVec;
            // FIXME scaled base blocks cause the blueprint to be placed in the wrong location (this also could be caused by a bug in DumpVON command)
			if (isBaseScaled)
			{
				Logging.LogWarning($"Found blueprint with scaled base block for {cube.name} (id:{cube.cubeId}), this is not currently supported");
			}
			for (int i = 0; i < blueprint.Length; i++)
			{
				BlockColor blueprintBlockColor = ColorSpaceUtility.QuantizeToBlockColor(blueprint[i].color);
				BlockColors blockColor = blueprintBlockColor.Color == BlockColors.White && blueprintBlockColor.Darkness == 0 ? cube.color : blueprintBlockColor.Color;
				byte blockDarkness = blueprintBlockColor.Color == BlockColors.White && blueprintBlockColor.Darkness == 0 ? cube.darkness : blueprintBlockColor.Darkness;
				float3 bluePos = new float3(blueprint[i].position[0], blueprint[i].position[1], blueprint[i].position[2]);
				float3 blueScale = new float3(blueprint[i].scale[0], blueprint[i].scale[1], blueprint[i].scale[2]);
				float3 blueRot = new float3(blueprint[i].rotation[0], blueprint[i].rotation[1], blueprint[i].rotation[2]);
				float3 physicalLocation = (float3)(cubeQuaternion * bluePos) + actualPosition;// + (blueprintSizeRotated / 2);
				//physicalLocation.x += blueprintSize.x / 2;
				physicalLocation += (float3)(cubeQuaternion * (correctionVec));
				//physicalLocation.y -= (float)(RobotCommands.blockSize * scale / 2);
				//float3 physicalScale = (float3)(cubeQuaternion * blueScale); // this actually over-rotates when combined with rotation
				float3 physicalScale = blueScale;
				float3 physicalRotation = (cubeQuaternion * Quaternion.Euler(blueRot)).eulerAngles;
#if DEBUG
				Logging.MetaLog($"Placing blueprint block at {physicalLocation} rot{physicalRotation} scale{physicalScale}");
				Logging.MetaLog($"Location math check original:{bluePos} rotated: {(float3)(cubeQuaternion * bluePos)} actualPos: {actualPosition} result: {physicalLocation}");
				Logging.MetaLog($"Scale math check original:{blueScale} rotation: {(float3)cubeQuaternion.eulerAngles} result: {physicalScale}");
				Logging.MetaLog($"Rotation math check original:{blueRot} rotated: {(cubeQuaternion * Quaternion.Euler(blueRot))} result: {physicalRotation}");
#endif
				placedBlocks[i] = Block.PlaceNew(VoxelObjectNotationUtility.NameToEnum(blueprint[i].name),
				                                 physicalLocation,
				                                 physicalRotation,
				                                 blockColor,
				                                 blockDarkness,
				                                 scale: physicalScale);
			}
#if DEBUG
			Logging.MetaLog($"Placed {placedBlocks.Length} blocks for blueprint {cube.name} (id:{cube.cubeId})");
#endif
			return placedBlocks;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LoadBlueprintMap()
		{
			StreamReader bluemap = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Pixi.blueprints.json"));
			blueprintMap = JsonConvert.DeserializeObject<Dictionary<uint, BlockJsonInfo[]>>(bluemap.ReadToEnd());
		}
	}
}
