using System;
using System.Runtime.CompilerServices;
using System.Text;

using UnityEngine;

using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Utility;

using Pixi.Common;

namespace Pixi.Images
{
    public static class PixelUtility
    {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockInfo QuantizePixel(Color pixel)
        {
#if DEBUG
            Logging.MetaLog($"Color (r:{pixel.r}, g:{pixel.g}, b:{pixel.b})");
#endif
			BlockColor c = ColorSpaceUtility.QuantizeToBlockColor(pixel);

			BlockInfo result = new BlockInfo
            {
                block = pixel.a > 0.75 ? BlockIDs.AluminiumCube : BlockIDs.GlassCube,
                color = c.Color,
                darkness = c.Darkness,
                visible = pixel.a > 0.5f,
            };
#if DEBUG
            Logging.MetaLog($"Quantized {result.color} (b:{result.block} d:{result.darkness} v:{result.visible})");
#endif
            return result;
        }

        public static string HexPixel(Color pixel)
		{
			return "#"+ColorUtility.ToHtmlStringRGBA(pixel);
		}

		public static string TextureToString(Texture2D img)
		{
			StringBuilder imgString = new StringBuilder("<cspace=-0.13em><line-height=40%>");
			for (int y = img.height-1; y >= 0 ; y--) // text origin is top right, but img origin is bottom right
            {
				for (int x = 0; x < img.width; x++)
                {
                    Color pixel = img.GetPixel(x, y);
                    imgString.Append("<color=");
                    imgString.Append(HexPixel(pixel));
					imgString.Append(">");
                    imgString.Append("\u25a0");
                }
                imgString.Append("<br>");
            }
			imgString.Append("</color></cspace></line-height>");
			return imgString.ToString();
		}
    }
}
