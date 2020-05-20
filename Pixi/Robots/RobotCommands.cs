﻿using System;
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

namespace Pixi.Robots
{
    public static class RobotCommands
    {
		private static double blockSize = 0.2;

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
			for (int c = 0; c < cubes.Length; c++) // sometimes I wish this were C++
			{
				CubeInfo cube = cubes[c];
				float3 realPosition = (cube.position * (float)blockSize) + position;
				Block newBlock = Block.PlaceNew(cube.block, realPosition, cube.rotation, cube.color, cube.darkness, scale: cube.scale);
				// the goal is for this to never evaluate to true (ie all cubes are translated correctly)
				if (!string.IsNullOrEmpty(cube.placeholder) && cube.block == BlockIDs.TextBlock)
				{
					newBlock.Specialise<TextBlock>().Text = cube.placeholder;
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
            position.y += (float)blockSize;
            CubeInfo[] cubes = CubeUtility.ParseCubes(robot);
            for (int c = 0; c < cubes.Length; c++) // sometimes I wish this were C++
            {
                CubeInfo cube = cubes[c];
                float3 realPosition = (cube.position * (float)blockSize) + position;
                Block newBlock = Block.PlaceNew(cube.block, realPosition, cube.rotation, cube.color, cube.darkness, scale: cube.scale);
                // the goal is for this to never evaluate to true (ie all cubes are translated correctly)
                if (!string.IsNullOrEmpty(cube.placeholder) && cube.block == BlockIDs.TextBlock)
                {
                    newBlock.Specialise<TextBlock>().Text = cube.placeholder;
                }
            }
            Logging.CommandLog($"Placed {robot.name} by {robot.addedByDisplayName} ({cubes.Length} cubes) beside you");
		}
    }
}