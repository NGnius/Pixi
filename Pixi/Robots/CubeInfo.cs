using System;
using Unity.Mathematics;
using GamecraftModdingAPI.Blocks;

namespace Pixi.Robots
{
	public struct CubeInfo
    {
        // you can't inherit from structs in C#...
        // this is an extension of BlockInfo
		public BlockIDs block;

        public BlockColors color;

        public byte darkness;

        public bool visible;

        // additions
		public float3 rotation;

		public float3 position;

		public float3 scale;

		public string name;

		public uint cubeId;
    }
}
