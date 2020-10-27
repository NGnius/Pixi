using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Svelto.DataStructures;
using Unity.Mathematics;
using UnityEngine;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Players;
using GamecraftModdingAPI.Utility;
using Pixi.Common;

namespace Pixi.Robots
{
    public class RobotInternetImporter : Importer
    {
        public int Priority { get; } = -100;
        
        public bool Optimisable { get; } = false;

        public string Name { get; } = "RobocraftRobot~Spell";

        public BlueprintProvider BlueprintProvider { get; }
        
        public static int CubeSize = 3;

        public RobotInternetImporter()
        {
	        BlueprintProvider = new RobotBlueprintProvider(this);
        }
        
        public bool Qualifies(string name)
        {
            string[] extensions = name.Split('.');
            return extensions.Length == 1 
                   || !extensions[extensions.Length - 1].Contains(" ");
        }

        public BlockJsonInfo[] Import(string name)
        {
            // download robot data
			RobotStruct robot;
			try
			{
				RobotBriefStruct[] botList = RoboAPIUtility.ListRobots(name);
				if (botList.Length == 0)
					throw new Exception("Failed to find robot");
				robot = RoboAPIUtility.QueryRobotInfo(botList[0].itemId);
                
			}
			catch (Exception e)
			{
				Logging.CommandLogError($"Failed to download robot data. Reason: {e.Message}");
				Logging.MetaLog(e);
				return new BlockJsonInfo[0];
			}
			CubeInfo[] cubes = CubeUtility.ParseCubes(robot);
			// move bot closer to origin (since bots are rarely built at the garage bay origin of the bottom south-west corner)
			if (cubes.Length == 0)
			{
				Logging.CommandLogError($"Robot data contains no cubes");
				return new BlockJsonInfo[0];
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
			BlockJsonInfo[] blocks = new BlockJsonInfo[cubes.Length];
			for (int c = 0; c < cubes.Length; c++)
			{
				ref CubeInfo cube = ref cubes[c];
				float3 realPosition = ((cube.position - minPosition) * CommandRoot.BLOCK_SIZE * CubeSize);
				if (cube.block == BlockIDs.TextBlock && !string.IsNullOrEmpty(cube.name))
				{
					// TextBlock block ID means it's a placeholder
					blocks[c] = new BlockJsonInfo
					{
						color = ColorSpaceUtility.UnquantizeToArray(cube.color, cube.darkness),
						name = cube.cubeId.ToString(),
						position = ConversionUtility.Float3ToFloatArray(realPosition),
						rotation = ConversionUtility.Float3ToFloatArray(cube.rotation),
						scale = ConversionUtility.Float3ToFloatArray(cube.scale)
					};
				}
				else
				{
					blocks[c] = new BlockJsonInfo
					{
						color = ColorSpaceUtility.UnquantizeToArray(cube.color, cube.darkness),
						name = cube.block.ToString(),
						position = ConversionUtility.Float3ToFloatArray(realPosition),
						rotation = ConversionUtility.Float3ToFloatArray(cube.rotation),
						scale = ConversionUtility.Float3ToFloatArray(cube.scale * CubeSize)
					};
				}
			}

			return blocks;
        }

        public void PreProcess(string name, ref ProcessedVoxelObjectNotation[] blocks)
        {
	        Player p = new Player(PlayerType.Local);
	        float3 pos = p.Position;
	        for (int i = 0; i < blocks.Length; i++)
	        {
		        blocks[i].position += pos;
	        }
	        // set textblock colors (replace <color="white"> with <color=#HEX> in textblocks)
	        Regex pattern = new Regex("<color=((?:\"white\")|(?:white))>", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	        for (int i = 0; i < blocks.Length; i++)
	        {
		        if (blocks[i].block == BlockIDs.TextBlock)
		        {
			        // TODO this blindly replaces color tags anywhere in metadata, not just ones that will go in the TextBlock's text field
#if DEBUG
			        Logging.MetaLog($"Replacing text field in block with colour {blocks[i].color} with #{ColorUtility.ToHtmlStringRGBA(ColorSpaceUtility.UnquantizeToColor(blocks[i].color))}");
#endif
			        blocks[i].metadata = pattern.Replace(
				        blocks[i].metadata,
				        $"<color=#{ColorUtility.ToHtmlStringRGBA(ColorSpaceUtility.UnquantizeToColor(blocks[i].color))}>");
			        // NOTE: Regex.Replace replaces the whole match string only when there's a capture group (it's dumb, idk why).
			        // The non-capturing groups may be messing with .NET or something
		        }
	        }
        }

        public void PostProcess(string name, ref Block[] blocks)
        {
        }
    }
}