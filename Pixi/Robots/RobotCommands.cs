using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

using Unity.Mathematics;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Commands;
using GamecraftModdingAPI.Players;
using GamecraftModdingAPI.Utility;

using Pixi.Common;

namespace Pixi.Robots
{
    public static class RobotCommands
    {
		internal const double blockSize = 0.2;

		public static int CubeSize = 3;

        public static void CreateRobotFileCommand()
		{
			CommandBuilder.Builder()
						  .Name("PixiBotFile")
			              .Description("Converts a robot file from RCBUP into Gamecraft blocks. Larger robots will freeze your game until conversion completes.")
						  .Action<string>(ImportRobotFile)
						  .Build();
		}

        public static void CreateRobotCRFCommand()
		{
			CommandBuilder.Builder()
						  .Name("PixiBot")
						  .Description("Downloads a robot from Robocraft's Factory and converts it into Gamecraft blocks. Larger robots will freeze your game until conversion completes.")
						  .Action<string>(ImportRobotOnline)
						  .Build();
		}

        public static void CreatePartDumpCommand()
		{
			CommandBuilder.Builder()
						  .Name("DumpVON")
						  .Description("Dump a block structure to a JSON file compatible with Pixi's internal VON format")
			              .Action<string>(DumpBlockStructure)
						  .Build();
		}

        private static void ImportRobotFile(string filepath)
		{
			string file;
			try
			{
				file = File.ReadAllText(filepath);
			}
			catch (Exception e)
			{
				Logging.CommandLogError($"Failed to load robot data. Reason: {e.Message}");
                Logging.MetaLog(e);
				return;
			}
			RobotStruct? robot = CubeUtility.ParseRobotInfo(file);
			if (!robot.HasValue)
			{
				Logging.CommandLogError($"Failed to parse robot data. File format was not recognised.");
				return;
			}
			float3 position = new Player(PlayerType.Local).Position;
			position.y += (float)blockSize;
			CubeInfo[] cubes = CubeUtility.ParseCubes(robot.Value);
			Block[][] blocks = new Block[cubes.Length][];
			for (int c = 0; c < cubes.Length; c++) // sometimes I wish this were C++
			{
				CubeInfo cube = cubes[c];
				float3 realPosition = (cube.position * (float)blockSize * CubeSize) + position;
				if (cube.block == BlockIDs.TextBlock && !string.IsNullOrEmpty(cube.name))
				{
                    // TextBlock block ID means it's a placeholder
					blocks[c] = CubeUtility.BuildBlueprintOrTextBlock(cube, realPosition, CubeSize);
				}
				else
				{
					blocks[c] = new Block[] { Block.PlaceNew(cube.block, realPosition, cube.rotation, cube.color, cube.darkness, CubeSize) };
				}
			}
            // build placeholders
			// Note: this is a separate loop because everytime a new block is placed,
            // a slow Sync() call is required to access it's properties.
			// This way, one Sync() call is needed, instead of O(cubes.Length) calls
			for (int c = 0; c < cubes.Length; c++)
			{
				CubeInfo cube = cubes[c];
				// the goal is for this to never evaluate to true (ie all cubes are translated correctly)
				if (!string.IsNullOrEmpty(cube.name) && cube.block == BlockIDs.TextBlock && blocks[c].Length == 1)
                {
					//Logging.MetaLog($"Block is {blocks[c][0].Type} and was placed as {cube.block}");
					blocks[c][0].Specialise<TextBlock>().Text = cube.name;
                }
			}
			Logging.CommandLog($"Placed {robot.Value.name} by {robot.Value.addedByDisplayName} ({cubes.Length} cubes) beside you");
		}

        private static void ImportRobotOnline(string robotName)
		{
			Stopwatch timer = Stopwatch.StartNew();
			// download robot data
			RobotStruct robot;
			try
			{
				RobotBriefStruct[] botList = RoboAPIUtility.ListRobots(robotName);
				if (botList.Length == 0)
					throw new Exception("Failed to find robot");
				robot = RoboAPIUtility.QueryRobotInfo(botList[0].itemId);
                
			}
			catch (Exception e)
			{
				Logging.CommandLogError($"Failed to download robot data. Reason: {e.Message}");
				Logging.MetaLog(e);
				timer.Stop();
                return;
			}
			timer.Stop();
			Logging.MetaLog($"Completed API calls in {timer.ElapsedMilliseconds}ms");
            float3 position = new Player(PlayerType.Local).Position;
			position.y += (float)(blockSize * CubeSize * 3); // 3 is roughly the max height of any cube in RC
            CubeInfo[] cubes = CubeUtility.ParseCubes(robot);
			// move origin closer to player (since bots are rarely built at the garage bay origin)
			if (cubes.Length == 0)
			{
				Logging.CommandLogError($"Robot data contains no cubes");
				return;
			}
			float3 minPosition = cubes[0].position;
			for (int c = 0; c < cubes.Length; c++)
			{
				float3 cubePos = cubes[c].position;
				if (cubePos.x < minPosition.x)
				{
					minPosition.x = cubePos.x;
				}
				if (cubePos.y < minPosition.y)
                {
					minPosition.y = cubePos.y;
                }
				if (cubePos.z < minPosition.z)
                {
					minPosition.z = cubePos.z;
                }
			}
			Block[][] blocks = new Block[cubes.Length][];
            for (int c = 0; c < cubes.Length; c++) // sometimes I wish this were C++
            {
				CubeInfo cube = cubes[c];
				float3 realPosition = ((cube.position - minPosition) * (float)blockSize * CubeSize) + position;
                if (cube.block == BlockIDs.TextBlock && !string.IsNullOrEmpty(cube.name))
                {
                    // TextBlock block ID means it's a placeholder
					blocks[c] = CubeUtility.BuildBlueprintOrTextBlock(cube, realPosition, CubeSize);
                }
                else
                {
                    blocks[c] = new Block[] { Block.PlaceNew(cube.block, realPosition, cube.rotation, cube.color, cube.darkness, CubeSize) };
                }
			}
			int blockCount = 0;
			for (int c = 0; c < cubes.Length; c++)
            {
				CubeInfo cube = cubes[c];
                // the goal is for this to never evaluate to true (ie all cubes are translated correctly)
				if (!string.IsNullOrEmpty(cube.name) && cube.block == BlockIDs.TextBlock && blocks[c].Length == 1)
                {
					//Logging.MetaLog($"Block is {blocks[c][0].Type} and was placed as {cube.block}");
					blocks[c][0].Specialise<TextBlock>().Text = cube.name;
                }
				blockCount += blocks[c].Length;
            }
			Logging.CommandLog($"Placed {robot.name} by {robot.addedByDisplayName} beside you ({cubes.Length}RC -> {blockCount}GC)");
		}

        private static void DumpBlockStructure(string filename)
		{
			Player local = new Player(PlayerType.Local);
			Block baseBlock = local.GetBlockLookedAt();
			Block[] blocks = baseBlock.GetConnectedCubes();
			if (blocks.Length == 0) return;
			float3 basePos = baseBlock.Position;
			string von = VoxelObjectNotationUtility.SerializeBlocks(blocks, new float[] { basePos.x, basePos.y, basePos.z });
			File.WriteAllText(filename, von);
		}
    }
}
