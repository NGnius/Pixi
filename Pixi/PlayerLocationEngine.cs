using System;

using GamecraftModdingAPI.Utility;
using Svelto.ECS;
using Unity.Mathematics;
using RobocraftX.Physics;
using RobocraftX.Character;

namespace Pixi
{
	internal class PlayerLocationEngine : IApiEngine
	{
		public string Name => "PixiPlayerLocationGameEngine";

		public EntitiesDB entitiesDB { set; private get; }

		public void Dispose() {}

		public void Ready() {}

        public float3 GetPlayerLocation(uint playerId)
		{
			return entitiesDB.QueryEntity<RigidBodyEntityStruct>(playerId, CharacterExclusiveGroups.OnFootGroup).position;
		}
	}
}
