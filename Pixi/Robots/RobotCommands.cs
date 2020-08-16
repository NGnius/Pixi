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
	    public static void CreatePartDumpCommand()
		{
			CommandBuilder.Builder()
						  .Name("DumpVON")
						  .Description("Dump a block structure to a JSON file compatible with Pixi's internal VON format")
			              .Action<string>(DumpBlockStructure)
						  .Build();
		}

        private static void DumpBlockStructure(string filename)
		{
			Player local = new Player(PlayerType.Local);
			Block baseBlock = local.GetBlockLookedAt();
			Block[] blocks = baseBlock.GetConnectedCubes();
			bool isBaseScaled = !(baseBlock.Scale.x > 0 && baseBlock.Scale.x < 2 && baseBlock.Scale.y > 0 && baseBlock.Scale.y < 2 && baseBlock.Scale.z > 0 && baseBlock.Scale.z < 2);
			if (isBaseScaled)
			{
				Logging.CommandLogWarning($"Detected scaled base block. This is not currently supported");
			}
			float3 basePos = baseBlock.Position;
			string von = VoxelObjectNotationUtility.SerializeBlocks(blocks, new float[] { basePos.x, basePos.y, basePos.z });
			File.WriteAllText(filename, von);
		}
    }
}
