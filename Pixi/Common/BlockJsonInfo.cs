using System;

using Unity.Mathematics;

using GamecraftModdingAPI.Blocks;

namespace Pixi.Common
{
    public struct BlockJsonInfo
    {
		public string name;

		public float[] position;

		public float[] rotation;

		public float[] color;

		public float[] scale;

		internal ProcessedVoxelObjectNotation Process()
		{
			BlockIDs block = ConversionUtility.BlockIDsToEnum(name.Split('\t')[0]);
			return new ProcessedVoxelObjectNotation
			{
				block = block,
				blueprint = block == BlockIDs.Invalid,
				color = ColorSpaceUtility.QuantizeToBlockColor(color),
				metadata = name,
				position = ConversionUtility.FloatArrayToFloat3(position),
				rotation = ConversionUtility.FloatArrayToFloat3(rotation),
				scale = ConversionUtility.FloatArrayToFloat3(scale),
			};
		}

		public override string ToString()
		{
			return $"BlockJsonInfo {{ name:{name}, color:(r{color[0]},g{color[1]},b{color[2]}), position:({position[0]},{position[1]},{position[2]}), rotation:({rotation[0]},{rotation[1]},{rotation[2]}), scale:({scale[0]},{scale[1]},{scale[2]})}}";
		}
    }
}
