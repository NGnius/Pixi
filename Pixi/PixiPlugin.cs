using System;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Mathematics; // float3

using IllusionPlugin;
using GamecraftModdingAPI.Utility;

using Pixi.Common;
using Pixi.Images;
using Pixi.Robots;

namespace Pixi
{
	public class PixiPlugin : IEnhancedPlugin // the Illusion Plugin Architecture (IPA) will ignore classes that don't implement IPlugin'
	{
		public override string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name; // Pixi
		// To change the name, change the project's name

		public override string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        // To change the version, change <Version>#.#.#</Version> in Pixi.csproj

        // called when Gamecraft shuts down
		public override void OnApplicationQuit()
		{
            // Shutdown this mod
			Logging.LogDebug($"{Name} has shutdown");

            // Shutdown the Gamecraft modding API last
			GamecraftModdingAPI.Main.Shutdown();
		}

        // called when Gamecraft starts up
		public override void OnApplicationStart()
		{
            // Initialize the Gamecraft modding API first
			GamecraftModdingAPI.Main.Init();
			// check out the modding API docs here: https://mod.exmods.org/

			// Initialize Pixi mod
			CommandRoot root = new CommandRoot();
			// 2D Image Functionality
			root.Inject(new ImageCanvasImporter());
			root.Inject(new ImageTextBlockImporter());
			root.Inject(new ImageCommandImporter());
			// Robot functionality
			var robot = new RobotInternetImporter();
			root.Inject(robot);
			//RobotCommands.CreateRobotCRFCommand();
			//RobotCommands.CreateRobotFileCommand();
#if DEBUG
			// Development functionality
			RobotCommands.CreatePartDumpCommand();
			((RobotBlueprintProvider) robot.BlueprintProvider).AddDebugCommands();
#endif
		}
	}
}