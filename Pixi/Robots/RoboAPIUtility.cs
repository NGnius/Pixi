using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

using GamecraftModdingAPI.Utility;

namespace Pixi.Robots
{
    public static class RoboAPIUtility
    {
		private const string ROBOT_API_LIST_URL = "https://factory.robocraftgame.com/api/roboShopItems/list";

        private const string ROBOT_API_GET_URL = "https://factory.robocraftgame.com/api/roboShopItems/get/";

        private const string ROBOT_API_TOKEN = "Web eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJQdWJsaWNJZCI6IjEyMyIsIkRpc3BsYXlOYW1lIjoiVGVzdCIsIlJvYm9jcmFmdE5hbWUiOiJGYWtlQ1JGVXNlciIsIkZsYWdzIjpbXSwiaXNzIjoiRnJlZWphbSIsInN1YiI6IldlYiIsImlhdCI6MTU0NTIyMzczMiwiZXhwIjoyNTQ1MjIzNzkyfQ.ralLmxdMK9rVKPZxGng8luRIdbTflJ4YMJcd25dKlqg";

		public static RobotBriefStruct[] ListRobots(string searchFilter, int pageSize = 10, bool playerFilter = false)
		{
            // pageSize <= 2 seems to retrieve items unreliably
			string bodyJson = $"{{\"page\": 1, \"pageSize\": {pageSize}, \"order\": 0, \"playerFilter\": {playerFilter.ToString().ToLower()}, \"movementFilter\": \"100000,200000,300000,400000,500000,600000,700000,800000,900000,1000000,1100000,1200000\", \"movementCategoryFilter\": \"100000,200000,300000,400000,500000,600000,700000,800000,900000,1000000,1100000,1200000\", \"weaponFilter\": \"10000000,20000000,25000000,30000000,40000000,50000000,60000000,65000000,70100000,75000000\", \"weaponCategoryFilter\": \"10000000,20000000,25000000,30000000,40000000,50000000,60000000,65000000,70100000,75000000\", \"minimumCpu\": -1, \"maximumCpu\": -1, \"minimumRobotRanking\": 0, \"maximumRobotRanking\": 1000000000, \"textFilter\": \"{searchFilter}\", \"textSearchField\": 0, \"buyable\": true, \"prependFeaturedRobot\": false, \"featuredOnly\": false, \"defaultPage\": false}}";
            byte[] reqBody = Encoding.UTF8.GetBytes(bodyJson);
#if DEBUG
			Logging.MetaLog($"POST body\n{bodyJson}");
#endif
			// download robot list
			// FIXME this blocks main thread
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ROBOT_API_LIST_URL);
            // request
            request.Method = "POST";
            request.ContentLength = reqBody.Length;
			request.ContentType = "application/json";
            request.Headers.Add(HttpRequestHeader.Authorization, ROBOT_API_TOKEN);
			request.Accept = "application/json; charset=utf-8"; // HTTP Status 500 without
			Stream body;
            body = request.GetRequestStream();
            body.Write(reqBody, 0, reqBody.Length);
            body.Close();
            // response
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
			// regular Stream was unreliable
            // because they could read everything before everything was availabe
			StreamReader respReader = new StreamReader(response.GetResponseStream());
			string bodyStr = respReader.ReadToEnd();
			RobotListResponse rlr = JsonConvert.DeserializeObject<RobotListResponse>(bodyStr);
			return rlr.response.roboShopItems;
		}

		public static RobotStruct QueryRobotInfo(int robotId)
		{
			// download robot info
			// FIXME this blocks main thread
			string url = ROBOT_API_GET_URL + robotId.ToString();
			Logging.MetaLog(url);
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // request
            request.Method = "GET";
            request.ContentType = "application/json";
			request.Accept = "application/json; charset=utf-8"; // HTTP Status 500 without
            request.Headers.Add(HttpRequestHeader.Authorization, ROBOT_API_TOKEN);
            // response
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            // regular Stream was unreliable
			// because they could read everything before everything was availabe
			StreamReader body = new StreamReader(response.GetResponseStream());
			string bodyStr = body.ReadToEnd();
            response.Close();
            RobotInfoResponse rir = JsonConvert.DeserializeObject<RobotInfoResponse>(bodyStr);
			return rir.response;
		}
    }
}
