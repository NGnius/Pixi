using System;
namespace Pixi.Robots
{
	public struct RobotBriefStruct
    {
        public int itemId;

		public string itemName;

		public string itemDescription;

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

    public struct RobotList
    {
		public RobotBriefStruct[] roboShopItems;
    }

    public struct RobotListResponse
	{
		public RobotList response;

		public int statusCode;
	}

    public struct RobotInfoResponse
	{
		public RobotStruct response;

		public int statusCode;
	}
}
