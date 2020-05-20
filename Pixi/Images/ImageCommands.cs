﻿using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using UnityEngine;
using Unity.Mathematics;
using Svelto.ECS.Experimental;
using Svelto.ECS;

using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Commands;
using GamecraftModdingAPI.Players;
using GamecraftModdingAPI.Utility;
using GamecraftModdingAPI;

using Pixi.Common;

namespace Pixi.Images
{
    public static class ImageCommands
    {
		public const uint PIXEL_WARNING_THRESHOLD = 25_000;
		// hash length to display after Pixi in text block id field
		public const uint HASH_LENGTH = 6;

		private static double blockSize = 0.2;

		private static uint thiccness = 1;

		public static void CreateThiccCommand()
		{
			CommandBuilder.Builder()
                          .Name("PixiThicc")
                          .Description("Set the image thickness for Pixi2D. Use this if you'd like add depth to a 2D image after importing.")
			              .Action<int>((d) => {
							  if (d > 0)
							  {
								  thiccness = (uint)d;
							  }
							  else Logging.CommandLogError("");
			              })
                          .Build();
		}

		public static void CreateImportCommand()
		{
			CommandBuilder.Builder()
                          .Name("Pixi2D")
                          .Description("Converts an image to blocks. Larger images will freeze your game until conversion completes.")
                          .Action<string>(Pixelate2DFile)
                          .Build();
		}

        public static void CreateTextCommand()
		{
			CommandBuilder.Builder()
						  .Name("PixiText")
						  .Description("Converts an image to coloured text in a new text block. Larger images may cause save issues.")
						  .Action<string>(Pixelate2DFileToTextBlock)
						  .Build();
		}

		public static void CreateTextConsoleCommand()
		{
			CommandBuilder.Builder()
						  .Name("PixiConsole")
						  .Description("Converts an image to a ChangeTextBlockCommand in a new console block. The first parameter is the image filepath and the second parameter is the text block id. Larger images may cause save issues.")
			              .Action<string, string>(Pixelate2DFileToCommand)
						  .Build();
		}

		public static void Pixelate2DFile(string filepath)
        {
            // Load image file and convert to Gamecraft blocks
            Texture2D img = new Texture2D(64, 64);
            // load file into texture
            try
            {
                byte[] imgData = File.ReadAllBytes(filepath);
                img.LoadImage(imgData);
            }
            catch (Exception e)
            {
                Logging.CommandLogError($"Failed to load picture data. Reason: {e.Message}");
                Logging.MetaLog(e.Message + "\n" + e.StackTrace);
                return;
            }
			Logging.CommandLog($"Image size: {img.width}x{img.height}");
            float3 position = new Player(PlayerType.Local).Position;
            uint blockCount = 0;
            position.x += 1f;
			position.y += (float)blockSize;
            float zero_y = position.y;
            // convert the image to blocks
            // this groups same-colored pixels in the same column into a single block to reduce the block count
            // any further pixel-grouping optimisations (eg 2D grouping) risk increasing conversion time higher than O(x*y)
            for (int x = 0; x < img.width; x++)
            {
				BlockInfo qVoxel = new BlockInfo
                {
                    block = BlockIDs.AbsoluteMathsBlock, // impossible canvas block
                    color = BlockColors.Default,
                    darkness = 10,
                    visible = false,
                };
				float3 scale = new float3(1, 1, thiccness);
                position.x += (float)(blockSize);
                for (int y = 0; y < img.height; y++)
                {
                    //position.y += (float)blockSize;
                    Color pixel = img.GetPixel(x, y);
					BlockInfo qPixel = PixelUtility.QuantizePixel(pixel);
                    if (qPixel.darkness != qVoxel.darkness
                        || qPixel.color != qVoxel.color
                        || qPixel.visible != qVoxel.visible
                        || qPixel.block != qVoxel.block)
                    {
                        if (y != 0)
                        {
                            if (qVoxel.visible)
                            {
                                position.y = zero_y + (float)((y * blockSize + (y - scale.y) * blockSize) / 2);
                                Block.PlaceNew(qVoxel.block, position, color: qVoxel.color, darkness: qVoxel.darkness, scale: scale);
                                blockCount++;
                            }
							scale = new float3(1, 1, thiccness);
                        }
                        qVoxel = qPixel;
                    }
                    else
                    {
                        scale.y += 1;
                    }

                }
                if (qVoxel.visible)
                {
                    position.y = zero_y + (float)((img.height * blockSize + (img.height - scale.y) * blockSize) / 2);
                    Block.PlaceNew(qVoxel.block, position, color: qVoxel.color, darkness: qVoxel.darkness, scale: scale);
                    blockCount++;
                }
                //position.y = zero_y;
            }
            Logging.CommandLog($"Placed {img.width}x{img.height} image beside you ({blockCount} blocks total)");
			Logging.MetaLog($"Saved {(img.width * img.height) - blockCount} blocks ({blockCount / (img.width * img.height)}%) while placing {filepath}");
        }

        public static void Pixelate2DFileToTextBlock(string filepath)
		{
            // Thanks to TheGreenGoblin for the idea (and the working Python implementation for reference)
			// Load image file and convert to Gamecraft blocks
            Texture2D img = new Texture2D(64, 64);
            // load file into texture
            try
            {
                byte[] imgData = File.ReadAllBytes(filepath);
                img.LoadImage(imgData);
            }
            catch (Exception e)
            {
                Logging.CommandLogError($"Failed to load picture data. Reason: {e.Message}");
                Logging.MetaLog(e.Message + "\n" + e.StackTrace);
                return;
            }
            float3 position = new Player(PlayerType.Local).Position;
            position.x += 1f;
            position.y += (float)blockSize;
			string text = PixelUtility.TextureToString(img);
			TextBlock textBlock = TextBlock.PlaceNew(position, scale: new float3(Mathf.Ceil(img.width / 16), Mathf.Ceil(img.height / 16), 1));
			textBlock.Text = text;
			byte[] textHash;
			using (HashAlgorithm hasher = SHA256.Create())
				textHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(text));
			string textId = "Pixi_";
            // every byte converts to 2 hexadecimal characters so hash length needs to be halved
			for (int i = 0; i < HASH_LENGTH/2 && i < textHash.Length; i++)
			{
				textId += textHash[i].ToString("X2");
			}
			textBlock.TextBlockId = textId;
		}

        public static void Pixelate2DFileToCommand(string filepath, string textBlockId)
		{
            // Thanks to Nullpersonan for the idea
			// Load image file and convert to Gamecraft blocks
            Texture2D img = new Texture2D(64, 64);
            // load file into texture
            try
            {
                byte[] imgData = File.ReadAllBytes(filepath);
                img.LoadImage(imgData);
            }
            catch (Exception e)
            {
                Logging.CommandLogError($"Failed to load picture data. Reason: {e.Message}");
                Logging.MetaLog(e.Message + "\n" + e.StackTrace);
                return;
            }
            float3 position = new Player(PlayerType.Local).Position;
            position.x += 1f;
            position.y += (float)blockSize;
            float zero_y = position.y;
            string text = PixelUtility.TextureToString(img); // conversion
			ConsoleBlock console = ConsoleBlock.PlaceNew(position);
			// set console's command
			console.Command = "ChangeTextBlockCommand";
			console.Arg1 = textBlockId;
			console.Arg2 = text;
			console.Arg3 = "";
		}
    }
}