using System;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Mathematics; // float3

using IllusionPlugin;
using GamecraftModdingAPI;
using GamecraftModdingAPI.Commands;
using GamecraftModdingAPI.Utility;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Players;

using Pixi.Images;

namespace Pixi
{
	public class PixiPlugin : IPlugin // the Illusion Plugin Architecture (IPA) will ignore classes that don't implement IPlugin'
	{
		public string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name; // Pixi
		// To change the name, change the project's name

		public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(); // 0.1.0 (for now)
        // To change the version, change <Version>#.#.#</Version> in Pixi.csproj

        // called when Gamecraft shuts down
		public void OnApplicationQuit()
		{
            // Shutdown this mod
			GamecraftModdingAPI.Utility.Logging.LogDebug($"{Name} has shutdown");

            // Shutdown the Gamecraft modding API last
			GamecraftModdingAPI.Main.Shutdown();
		}

        // called when Gamecraft starts up
		public void OnApplicationStart()
		{
            // Initialize the Gamecraft modding API first
			GamecraftModdingAPI.Main.Init();
			// check out the modding API docs here: https://mod.exmods.org/

			// Initialize Pixi mod
			// 2D image functionality
			ImageCommands.CreateThiccCommand();
			ImageCommands.CreateImportCommand();
			ImageCommands.CreateTextCommand();
			ImageCommands.CreateTextConsoleCommand();
            
			GamecraftModdingAPI.Utility.Logging.LogDebug($"{Name} has started up");
		}

        // unused methods

		public void OnFixedUpdate() { } // called once per physics update

		public void OnLevelWasInitialized(int level) { } // called after a level is initialized

		public void OnLevelWasLoaded(int level) { } // called after a level is loaded

		public void OnUpdate() { } // called once per rendered frame (frame update)
	}
}