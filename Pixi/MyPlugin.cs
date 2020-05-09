using System.Reflection;

using IllusionPlugin;
// using GamecraftModdingAPI;
using GamecraftModdingAPI.Commands;

namespace Pixi
{
	public class MyPlugin : IPlugin // the Illusion Plugin Architecture (IPA) will ignore classes that don't implement IPlugin'
	{
		public string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name; // Pixi by default
		// To change the name, change the project's name

		public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(); // 0.0.1 by default
        // To change the version, change <Version>0.0.1</Version> in Pixi.csproj

		private static readonly string helloWorldCommandName = "HelloWorld"; // command name

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

			// Initialize this mod
            // create SimpleCustomCommandEngine
            // this writes "Hello modding world!" when you execute it in Gamecraft's console
            // (use the forward-slash key '/' to open the console in Gamecraft when in a game)
			SimpleCustomCommandEngine helloWorldCommand = new SimpleCustomCommandEngine(
				() => { GamecraftModdingAPI.Utility.Logging.CommandLog("Hello modding world!"); }, // command action
                // also try using CommandLogWarning or CommandLogError instead of CommandLog
				helloWorldCommandName, // command name (used to invoke it in the console)
                "Says Hello modding world!" // command description (displayed when help command is executed)
			); // this command can also be executed using the Command Computer

            // register the command so the modding API knows about it
			CommandManager.AddCommand(helloWorldCommand);

			GamecraftModdingAPI.Utility.Logging.LogDebug($"{Name} has started up");
		}

        // unused methods

		public void OnFixedUpdate() { } // called once per physics update

		public void OnLevelWasInitialized(int level) { } // called after a level is initialized

		public void OnLevelWasLoaded(int level) { } // called after a level is loaded

		public void OnUpdate() { } // called once per rendered frame (frame update)
	}
}