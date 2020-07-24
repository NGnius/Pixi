using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Unity.Mathematics;

using GamecraftModdingAPI.Blocks;

namespace Pixi.Common
{
    public static class ConversionUtility
    {
        private static Dictionary<string, BlockIDs> blockEnumMap = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void loadBlockEnumMap()
        {
            blockEnumMap = new Dictionary<string, BlockIDs>();
            foreach(BlockIDs e in Enum.GetValues(typeof(BlockIDs)))
            {
                blockEnumMap[e.ToString()] = e;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockIDs BlockIDsToEnum(string name)
        {
            if (blockEnumMap == null) loadBlockEnumMap();
            if (blockEnumMap.ContainsKey(name)) return blockEnumMap[name];
            return BlockIDs.Invalid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 FloatArrayToFloat3(float[] vec)
        {
            if (vec.Length < 3) return float3.zero;
            return new float3(vec[0], vec[1], vec[2]);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] Float3ToFloatArray(float3 vec)
        {
            return new float[3] {vec.x, vec.y, vec.z};
        }
    }
}