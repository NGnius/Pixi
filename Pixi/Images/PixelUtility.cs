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
		public static string HexPixel(Color pixel)
		{
			return "#"+ColorUtility.ToHtmlStringRGBA(pixel);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color PixelHex(string hex)
		{
			if (ColorUtility.TryParseHtmlString(hex, out Color result))
			{
				return result;
			}
			return default;
		}

		public static string TextureToString(Texture2D img)
		{
			StringBuilder imgString = new StringBuilder("<cspace=-0.13em><line-height=40%>");
			bool lastPixelAssigned = false;
			Color lastPixel = new Color();
			for (int y = img.height-1; y >= 0 ; y--) // text origin is top right, but img origin is bottom right
            {
				for (int x = 0; x < img.width; x++)
                {
                    Color pixel = img.GetPixel(x, y);
                    if (!lastPixelAssigned || lastPixel != pixel)
                    {
	                    imgString.Append("<color=");
	                    imgString.Append(HexPixel(pixel));
	                    imgString.Append(">");
	                    lastPixel = pixel;
	                    if (!lastPixelAssigned)
	                    {
		                    lastPixelAssigned = true;
	                    }
                    }
                    imgString.Append("\u25a0");
                }
                imgString.Append("<br>");
            }
			imgString.Append("</color></cspace></line-height>");
			return imgString.ToString();
		}
    }
}
