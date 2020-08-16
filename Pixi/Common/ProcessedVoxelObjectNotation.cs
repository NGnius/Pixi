using Unity.Mathematics;

using GamecraftModdingAPI.Blocks;

namespace Pixi.Common
{
    public struct ProcessedVoxelObjectNotation
    {
        public BlockIDs block;

        public BlockColor color;

        public bool blueprint;
        
        public float3 position;

        public float3 rotation;

        public float3 scale;

        public string metadata;

        internal BlockJsonInfo VoxelObjectNotation()
        {
            return new BlockJsonInfo
            {
                name = block == BlockIDs.Invalid ? metadata.Split(' ')[0] : block.ToString(),
                color = ColorSpaceUtility.UnquantizeToArray(color),
                position = ConversionUtility.Float3ToFloatArray(position),
                rotation = ConversionUtility.Float3ToFloatArray(rotation),
                scale = ConversionUtility.Float3ToFloatArray(scale),
            };
        }

        public override string ToString()
        {
            return $"ProcessedVoxelObjectNotation {{ block:{block}, color:{color.Color}-{color.Darkness}, blueprint:{blueprint}, position:{position}, rotation:{rotation}, scale:{scale}}} ({metadata})";
        }
    }
}