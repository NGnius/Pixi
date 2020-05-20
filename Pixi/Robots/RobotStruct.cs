using System;
namespace Pixi.Robots
{
    public struct RobotStruct
    {
		public int id;

		public string name;

		public string description;

		public string thumbnail;

		public string addedBy;

		public string addedByDisplayName;

		public int cpu;

		public int totalRobotRanking;

		public string cubeData;

		public string colourData;

		public bool featured;

		public string cubeAmounts; // this is sent incorrectly by the API server (it's actually a Dictionary<string, int>)
    }
}
