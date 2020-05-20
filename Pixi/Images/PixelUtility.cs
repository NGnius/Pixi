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
            BlockColors color = BlockColors.Default;
            int darkness = 0;
            bool force = false;
#if DEBUG
            Logging.MetaLog($"Color (r:{pixel.r}, g:{pixel.g}, b:{pixel.b})");
#endif
            if (Mathf.Abs(pixel.r - pixel.g) <= pixel.r * 0.1f && Mathf.Abs(pixel.r - pixel.b) <= pixel.r * 0.1f)
            {
                color = BlockColors.White;
                darkness = (int)(10 - ((pixel.r + pixel.g + pixel.b) * 3.5));
                //Logging.MetaDebugLog($"Color (r:{pixel.r}, g:{pixel.g}, b:{pixel.b})");
            }
            else if (pixel.r >= pixel.g && pixel.r >= pixel.b)
            {
                // Red is highest
                if ((pixel.r - pixel.g) > pixel.r * 0.65 && (pixel.r - pixel.b) > pixel.r * 0.55)
                {
                    // Red is much higher than other pixels
                    darkness = (int)(9 - (pixel.r * 8.01));
                    color = BlockColors.Red;
                }
                else if ((pixel.g - pixel.b) > pixel.g * 0.25)
                {
                    // Green is much higher than blue
                    if ((pixel.r - pixel.g) < pixel.r * 0.8)
                    {
                        darkness = (int)(10 - ((pixel.r * 2.1 + pixel.g) * 2.1));
                        color = BlockColors.Orange;
                    }
                    else
                    {
                        darkness = (int)(10 - ((pixel.r * 2.1 + pixel.g) * 2.2));
                        color = BlockColors.Yellow;
                    }

                }
                else if ((pixel.b - pixel.g) > pixel.b * 0.3)
                {
                    // Blue is much higher than green
                    darkness = (int)(10 - ((pixel.r + pixel.b) * 5.0));
                    color = BlockColors.Purple;
                }
                else
                {
                    // Green is close strength to blue
                    darkness = (int)(10 - ((pixel.r * 2.1 + pixel.g + pixel.b) * 2.5));
                    color = darkness < 6 ? BlockColors.Pink : BlockColors.Orange;
                    force = true;
                }
            }
            else if (pixel.g >= pixel.r && pixel.g >= pixel.b)
            {
                // Green is highest
                if ((pixel.g - pixel.r) > pixel.g * 0.6 && (pixel.g - pixel.b) > pixel.g * 0.48)
                {
                    // Green is much higher than other pixels
                    darkness = (int)(10 - (pixel.g * 10.1));
                    color = BlockColors.Green;
                }
                else if ((pixel.r - pixel.b) > pixel.r * 0.3)
                {
                    // Red is much higher than blue
                    darkness = (int)(10 - ((pixel.r + pixel.g) * 5.1));
                    color = BlockColors.Yellow;
                }
                else if ((pixel.b - pixel.r) > pixel.b * 0.2)
                {
                    // Blue is much higher than red
                    darkness = (int)(9 - ((pixel.g + pixel.b) * 5.1));
                    color = BlockColors.Aqua;
                }
                else
                {
                    // Red is close strength to blue
                    darkness = (int)(10 - ((pixel.r + pixel.g * 2.2 + pixel.b) * 2.9));
                    color = BlockColors.Lime;
                }
            }
            else if (pixel.b >= pixel.g && pixel.b >= pixel.r)
            {
                // Blue is highest
                if ((pixel.b - pixel.g) > pixel.b * 0.6 && (pixel.b - pixel.r) > pixel.b * 0.6)
                {
                    // Blue is much higher than other pixels
                    darkness = (int)(10 - (pixel.b * 10.1));
                    color = BlockColors.Blue;
                }
                else if ((pixel.g - pixel.r) > pixel.g * 0.3)
                {
                    // Green is much higher than red
                    darkness = (int)(10 - ((pixel.g + pixel.b) * 5.1));
                    if (darkness == 4 || darkness == 5) darkness = 0;
                    else if (darkness < 3) darkness = 4;
                    color = BlockColors.Aqua;
                }
                else if ((pixel.r - pixel.g) > pixel.r * 0.3)
                {
                    // Red is much higher than green
                    darkness = (int)(10 - ((pixel.r + pixel.b) * 5.0));
                    color = BlockColors.Purple;
                }
                else
                {
                    // Green is close strength to red
                    darkness = (int)(10 - ((pixel.r + pixel.g + pixel.b * 2.2) * 3.0));
                    color = BlockColors.Aqua;
                }
            }
            // level 9 is not darker than lvl 8
            if (darkness > 8 && !force) darkness = 8;
            // darkness 0 is the most saturated (it's not just the lightest)
            if (darkness < 0) darkness = 0;

			BlockInfo result = new BlockInfo
            {
                block = pixel.a > 0.75 ? BlockIDs.AluminiumCube : BlockIDs.GlassCube,
                color = color,
                darkness = (byte)darkness,
                visible = pixel.a > 0.5f,
            };
#if DEBUG
            Logging.MetaLog($"Quantized {color} (b:{result.block} d:{result.darkness} v:{result.visible})");
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
