using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using GamecraftModdingAPI.App;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Tasks;
using GamecraftModdingAPI.Utility;
using MiniJSON;
using Svelto.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Pixi.Common
{
	public static class ColorSpaceUtility
	{
		private const float optimal_delta = 0.2f;

		private static Dictionary<BlockColor, float[]> colorMap = null;

		private static Dictionary<byte, BlockColor> botColorMap = null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockColor QuantizeToBlockColor(Color pixel)
		{
			//if (colorMap == null) BuildColorMap();
			float[] closest = new float[3] { 1, 1, 1 };
			BlockColor c = new BlockColor
			{
				Color = BlockColors.Default,
				Darkness = 0,
			};
			BlockColor[] keys = colorMap.Keys.ToArray();
			float geometricClosest = float.MaxValue;
			for (int k = 0; k < keys.Length; k++)
			{
				float[] color = colorMap[keys[k]];
				float[] distance = new float[3] { Math.Abs(pixel.r - color[0]), Math.Abs(pixel.g - color[1]), Math.Abs(pixel.b - color[2]) };
				float dist = Mathf.Sqrt(Mathf.Pow(distance[0], 2) + Mathf.Pow(distance[1], 2) + Mathf.Pow(distance[2], 2));
				if (dist < geometricClosest)
				{
					c = keys[k];
					closest = distance;
					geometricClosest = Mathf.Sqrt(Mathf.Pow(closest[0], 2) + Mathf.Pow(closest[1], 2) + Mathf.Pow(closest[2], 2));
					if (geometricClosest < optimal_delta)
					{
#if DEBUG
						Logging.MetaLog($"Final delta ({closest[0]},{closest[1]},{closest[2]}) t:{geometricClosest}");
#endif
						return c;
					}
				}
			}
#if DEBUG
			Logging.MetaLog($"Final delta ({closest[0]},{closest[1]},{closest[2]}) t:{geometricClosest}");
#endif
			return c;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockColor QuantizeToBlockColor(byte cubeColorEnum)
        {
			if (botColorMap == null) BuildBotColorMap();
			return botColorMap[cubeColorEnum];
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockColor QuantizeToBlockColor(float[] pixel)
		{
			if (pixel.Length < 3 || pixel[0] < 0 || pixel[1] < 0 || pixel[2] < 0)
			{
				return new BlockColor
				{
					Color = BlockColors.Default,
					Darkness = 0,
				};
			}
			return QuantizeToBlockColor(new Color(pixel[0], pixel[1], pixel[2]));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float[] UnquantizeToArray(BlockColor c)
		{
			//if (colorMap == null) BuildColorMap();
			return colorMap[c];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float[] UnquantizeToArray(BlockColors color, byte darkness = 0)
        {
			return UnquantizeToArray(new BlockColor
			{
				Color = color,
				Darkness = darkness,
			});
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color UnquantizeToColor(BlockColor c)
        {
			float[] t = UnquantizeToArray(c);
			return new Color(t[0], t[1], t[2]);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color UnquantizeToColor(BlockColors color, byte darkness = 0)
        {
			return UnquantizeToColor(new BlockColor
			{
				Color = color,
				Darkness = darkness,
			});
        }

		public static void LoadColorMenuEvent(object caller, MenuEventArgs info)
		{
			Scheduler.Schedule(new AsyncRunner());
		}

		private static void BuildColorMap()
        {
	        // old manual version for building color map
            colorMap = new Dictionary<BlockColor, float[]>();
            // this was done manually -- never again
            // White
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 0 }] = new float[3] { 1f, 1f, 1f};
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 1 }] = new float[3] { 0.88f, 0.98f, 0.99f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 2 }] = new float[3] { 0.80f, 0.89f, 0.99f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 3 }] = new float[3] { 0.746f, 0.827f, 0.946f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 4 }] = new float[3] { 0.71f, 0.789f, 0.888f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 5 }] = new float[3] { 0.597f, 0.664f, 0.742f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 6 }] = new float[3] { 0.484f, 0.535f, 0.61f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 7 }] = new float[3] { 0.355f, 0.39f, 0.449f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 8 }] = new float[3] { 0f, 0f, 0f };
			colorMap[new BlockColor { Color = BlockColors.White, Darkness = 9 }] = new float[3] { 0.581f, 0.643f, 0.745f };
            // Pink
			colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 0 }] = new float[3] { 1f, 0.657f, 1f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 1 }] = new float[3] { 0.912f, 0.98f, 0.993f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 2 }] = new float[3] { 0.897f, 0.905f, 0.991f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 3 }] = new float[3] { 0.892f, 0.776f, 0.988f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 4 }] = new float[3] { 0.898f, 0.698f, 0.992f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 5 }] = new float[3] { 0.875f, 0.267f, 0.882f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 6 }] = new float[3] { 0.768f, 0.199f, 0.767f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 7 }] = new float[3] { 0.628f, 0.15f, 0.637f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 8 }] = new float[3] { 0.435f, 0.133f, 0.439f };
            colorMap[new BlockColor { Color = BlockColors.Pink, Darkness = 9 }] = new float[3] { 0.726f, 0.659f, 0.871f };
            // Purple
			colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 0 }] = new float[3] { 0.764f, 0.587f, 1f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 1 }] = new float[3] { 0.893f, 0.966f, 0.992f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 2 }] = new float[3] { 0.842f, 0.877f, 0.991f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 3 }] = new float[3] { 0.794f, 0.747f, 0.99f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 4 }] = new float[3] { 0.783f, 0.669f, 0.992f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 5 }] = new float[3] { 0.636f, 0.249f, 0.991f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 6 }] = new float[3] { 0.548f, 0.18f, 0.896f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 7 }] = new float[3] { 0.441f, 0.152f, 0.726f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 8 }] = new float[3] { 0.308f, 0.135f, 0.498f };
            colorMap[new BlockColor { Color = BlockColors.Purple, Darkness = 9 }] = new float[3] { 0.659f, 0.646f, 0.909f };
            // Blue
			colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 0 }] = new float[3] { 0.449f, 0.762f, 1f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 1 }] = new float[3] { 0.856f, 0.971f, 0.992f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 2 }] = new float[3] { 0.767f, 0.907f, 0.989f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 3 }] = new float[3] { 0.642f, 0.836f, 0.992f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 4 }] = new float[3] { 0.564f, 0.812f, 0.989f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 5 }] = new float[3] { 0.211f, 0.621f, 0.989f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 6 }] = new float[3] { 0.143f, 0.525f, 0.882f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 7 }] = new float[3] { 0.114f, 0.410f, 0.705f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 8 }] = new float[3] { 0.116f, 0.289f, 0.481f };
            colorMap[new BlockColor { Color = BlockColors.Blue, Darkness = 9 }] = new float[3] { 0.571f, 0.701f, 0.901f };
            // Aqua
			colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 0 }] = new float[3] { 0.408f, 0.963f, 1f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 1 }] = new float[3] { 0.838f, 0.976f, 0.990f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 2 }] = new float[3] { 0.747f, 0.961f, 0.994f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 3 }] = new float[3] { 0.605f, 0.948f, 0.990f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 4 }] = new float[3] { 0.534f, 0.954f, 0.993f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 5 }] = new float[3] { 0.179f, 0.841f, 0.991f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 6 }] = new float[3] { 0.121f, 0.719f, 0.868f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 7 }] = new float[3] { 0.117f, 0.574f, 0.687f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 8 }] = new float[3] { 0.116f, 0.399f, 0.478f };
            colorMap[new BlockColor { Color = BlockColors.Aqua, Darkness = 9 }] = new float[3] { 0.556f, 0.768f, 0.901f };
            // Green
			colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 0 }] = new float[3] { 0.344f, 1f, 0.579f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 1 }] = new float[3] { 0.823f, 0.977f, 0.994f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 2 }] = new float[3] { 0.731f, 0.966f, 0.958f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 3 }] = new float[3] { 0.643f, 0.964f, 0.873f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 4 }] = new float[3] { 0.498f, 0.961f, 0.721f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 5 }] = new float[3] { 0.176f, 0.853f, 0.415f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 6 }] = new float[3] { 0.120f, 0.728f, 0.350f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 7 }] = new float[3] { 0.105f, 0.560f, 0.264f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 8 }] = new float[3] { 0.122f, 0.392f, 0.221f };
            colorMap[new BlockColor { Color = BlockColors.Green, Darkness = 9 }] = new float[3] { 0.542f, 0.771f, 0.717f };
            // Lime
			colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 0 }] = new float[3] { 0.705f, 1f, 0.443f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 1 }] = new float[3] { 0.869f, 0.978f, 0.991f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 2 }] = new float[3] { 0.815f, 0.967f, 0.932f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 3 }] = new float[3] { 0.778f, 0.962f, 0.821f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 4 }] = new float[3] { 0.753f, 0.964f, 0.631f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 5 }] = new float[3] { 0.599f, 0.855f, 0.268f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 6 }] = new float[3] { 0.505f, 0.712f, 0.201f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 7 }] = new float[3] { 0.376f, 0.545f, 0.185f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 8 }] = new float[3] { 0.268f, 0.379f, 0.172f };
            colorMap[new BlockColor { Color = BlockColors.Lime, Darkness = 9 }] = new float[3] { 0.631f, 0.768f, 0.690f };
            // Yellow
			colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 0 }] = new float[3] { 0.893f, 1f, 0.457f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 1 }] = new float[3] { 0.887f, 0.981f, 0.995f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 2 }] = new float[3] { 0.878f, 0.971f, 0.920f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 3 }] = new float[3] { 0.874f, 0.964f, 0.802f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 4 }] = new float[3] { 0.875f, 0.964f, 0.619f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 5 }] = new float[3] { 0.771f, 0.846f, 0.246f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 6 }] = new float[3] { 0.638f, 0.703f, 0.192f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 7 }] = new float[3] { 0.477f, 0.522f, 0.142f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 8 }] = new float[3] { 0.330f, 0.363f, 0.151f };
            colorMap[new BlockColor { Color = BlockColors.Yellow, Darkness = 9 }] = new float[3] { 0.693f, 0.763f, 0.678f };
            // Orange
			colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 0 }] = new float[3] { 0.891f, 0.750f, 0.423f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 1 }] = new float[3] { 0.883f, 0.948f, 0.992f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 2 }] = new float[3] { 0.877f, 0.873f, 0.894f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 3 }] = new float[3] { 0.878f, 0.831f, 0.771f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 4 }] = new float[3] { 0.886f, 0.801f, 0.595f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 5 }] = new float[3] { 0.777f, 0.621f, 0.241f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 6 }] = new float[3] { 0.637f, 0.507f, 0.168f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 7 }] = new float[3] { 0.466f, 0.364f, 0.123f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 8 }] = new float[3] { 0.323f, 0.266f, 0.138f };
            colorMap[new BlockColor { Color = BlockColors.Orange, Darkness = 9 }] = new float[3] { 0.689f, 0.672f, 0.667f };
            // Red
			colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 0 }] = new float[3] { 0.890f, 0.323f, 0.359f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 1 }] = new float[3] { 0.879f, 0.863f, 0.987f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 2 }] = new float[3] { 0.872f, 0.758f, 0.868f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 3 }] = new float[3] { 0.887f, 0.663f, 0.756f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 4 }] = new float[3] { 0.903f, 0.546f, 0.608f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 5 }] = new float[3] { 0.785f, 0.222f, 0.222f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 6 }] = new float[3] { 0.641f, 0.155f, 0.152f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 7 }] = new float[3] { 0.455f, 0.105f, 0.108f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 8 }] = new float[3] { 0.320f, 0.121f, 0.133f };
            colorMap[new BlockColor { Color = BlockColors.Red, Darkness = 9 }] = new float[3] { 0.687f, 0.571f, 0.661f };
            // default
            colorMap[new BlockColor { Color = BlockColors.Default, Darkness = 0 }] = new float[3] { -1f, -1f, -1f };
        }

        private static void BuildBotColorMap()
		{
			botColorMap = new Dictionary<byte, BlockColor>();
            // standard colours
			botColorMap[0] = new BlockColor { Color = BlockColors.White, Darkness = 0 };
			botColorMap[1] = new BlockColor { Color = BlockColors.White, Darkness = 6 };
			botColorMap[4] = new BlockColor { Color = BlockColors.White, Darkness = 8 };
			botColorMap[5] = new BlockColor { Color = BlockColors.Red, Darkness = 5 };
			botColorMap[2] = new BlockColor { Color = BlockColors.Orange, Darkness = 0 };
			botColorMap[6] = new BlockColor { Color = BlockColors.Yellow, Darkness = 0 };
			botColorMap[7] = new BlockColor { Color = BlockColors.Green, Darkness = 5 };
			botColorMap[3] = new BlockColor { Color = BlockColors.Aqua, Darkness = 5 };
			botColorMap[9] = new BlockColor { Color = BlockColors.Blue, Darkness = 5 };
			botColorMap[10] = new BlockColor { Color = BlockColors.Purple, Darkness = 5 };
            // premium colours
			botColorMap[16] = new BlockColor { Color = BlockColors.Red, Darkness = 0 };
			botColorMap[17] = new BlockColor { Color = BlockColors.Red, Darkness = 7 };
			botColorMap[11] = new BlockColor { Color = BlockColors.Orange, Darkness = 6 };
			botColorMap[18] = new BlockColor { Color = BlockColors.Purple, Darkness = 9 };
			botColorMap[19] = new BlockColor { Color = BlockColors.Pink, Darkness = 9 };
			botColorMap[20] = new BlockColor { Color = BlockColors.Orange, Darkness = 5 };
			botColorMap[14] = new BlockColor { Color = BlockColors.Yellow, Darkness = 3 };
			botColorMap[21] = new BlockColor { Color = BlockColors.Green, Darkness = 7 };
			botColorMap[22] = new BlockColor { Color = BlockColors.Lime, Darkness = 8 };
			botColorMap[13] = new BlockColor { Color = BlockColors.Green, Darkness = 6 };
			botColorMap[12] = new BlockColor { Color = BlockColors.Lime, Darkness = 5 };
            // blue gang
			botColorMap[23] = new BlockColor { Color = BlockColors.Blue, Darkness = 8 };
			botColorMap[24] = new BlockColor { Color = BlockColors.Aqua, Darkness = 8 };
			botColorMap[25] = new BlockColor { Color = BlockColors.Blue, Darkness = 7 };
			botColorMap[26] = new BlockColor { Color = BlockColors.White, Darkness = 5 };
			botColorMap[27] = new BlockColor { Color = BlockColors.White, Darkness = 4 };
			botColorMap[28] = new BlockColor { Color = BlockColors.Aqua, Darkness = 4 };
			botColorMap[29] = new BlockColor { Color = BlockColors.Purple, Darkness = 8 };
            // purples & pinks
			botColorMap[30] = new BlockColor { Color = BlockColors.Pink, Darkness = 0 };
			botColorMap[8] = new BlockColor { Color = BlockColors.Pink, Darkness = 5 };
			botColorMap[31] = new BlockColor { Color = BlockColors.Pink, Darkness = 4 };
			botColorMap[15] = new BlockColor { Color = BlockColors.Red, Darkness = 3 };
		}

        private class AsyncRunner : ISchedulable
        {
	        public IEnumerator<TaskContract> Run()
	        {
		        AsyncOperationHandle<TextAsset> asyncHandle = Addressables.LoadAssetAsync<TextAsset>("colours");
		        yield return asyncHandle.Continue();
		        Dictionary<string, object> colourData = Json.Deserialize(asyncHandle.Result.text) as Dictionary<string, object>;
		        if (colourData == null) yield break;
		        Client.EnterMenu -= LoadColorMenuEvent;
		        // Logging.MetaLog((List<object>)((colourData["Colours"] as Dictionary<string, object>)["Data"] as Dictionary<string, object>)["Slots"]);
		        // Generate color map
		        List<object> hexColors =
			        (((colourData["Colours"] as Dictionary<string, object>)?["Data"] as Dictionary<string, object>)?
				        ["Slots"] as List<object>);
		        int count = 0;
		        colorMap = new Dictionary<BlockColor, float[]>();
		        for (byte d = 0; d < 10; d++)
		        {
					foreach (BlockColors c in Enum.GetValues(typeof(BlockColors)))
					{
						if (c != BlockColors.Default)
						{
							BlockColor colorStruct = new BlockColor
							{
								Color = c,
								Darkness = d,
							};
							Color pixel = Images.PixelUtility.PixelHex((string)hexColors[count]);
							colorMap[colorStruct] = new float[] {pixel.r, pixel.g, pixel.b};
							count++;
						}
					}
		        }
	        }
        }
    }
}
