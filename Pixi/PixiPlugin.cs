﻿using System;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Mathematics; // float3

using IllusionPlugin;
// using GamecraftModdingAPI;
using GamecraftModdingAPI.Commands;
using GamecraftModdingAPI.Utility;
using GamecraftModdingAPI.Blocks;

namespace Pixi
{
	public class PixiPlugin : IPlugin // the Illusion Plugin Architecture (IPA) will ignore classes that don't implement IPlugin'
	{
		private const uint PIXEL_WARNING_THRESHOLD = 25_000;

		public string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name; // Pixi
		// To change the name, change the project's name

		public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(); // 0.1.0 (for now)
        // To change the version, change <Version>#.#.#</Version> in Pixi.csproj

		private uint width = 64;
		private uint height = 64;

		private double blockSize = 0.2;

		private PlayerLocationEngine playerLocationEngine = new PlayerLocationEngine();

        // called when Gamecraft shuts down
		public void OnApplicationQuit()
		{
            // Shutdown this mod
			GamecraftModdingAPI.Utility.Logging.LogDebug($"{Name} has shutdown");

            // Shutdown the Gamecraft modding API last
			GamecraftModdingAPI.Main.Shutdown();
		}

        // called when Gamecraft starts up
		public void OnApplicationStart()
		{
            // Initialize the Gamecraft modding API first
			GamecraftModdingAPI.Main.Init();
			// check out the modding API docs here: https://mod.exmods.org/

			// Initialize Pixi mod
            // create SimpleCustomCommandEngine for 2D image importing
			SimpleCustomCommandEngine<string> pixelate2DCommand = new SimpleCustomCommandEngine<string>(
				pixelate2DFile, // command action
				"Pixi2D", // command name (used to invoke it in the console)
                "Converts an image to blocks.\nLarger images will freeze your game until conversion completes. (Pixi)" // command description (displayed when help command is executed)
			);

			SimpleCustomCommandEngine<string> pixelate3DCommand = new SimpleCustomCommandEngine<string>(
                pixelate3DFile, // command action
                "Pixi3D", // command name (used to invoke it in the console)
                "Converts a 3D model to blocks.\nLarger models will freeze your game until conversion completes. (Pixi)" // command description (displayed when help command is executed)
            );

			SimpleCustomCommandEngine<uint, uint> scaleCommand = new SimpleCustomCommandEngine<uint, uint>(
				setScale, // command action
                "PixiScale", // command name (used to invoke it in the console)
				"Sets the image scale factor for Pixi2D.\nBigger images take longer to convert. (Pixi)" // command description (displayed when help command is executed)
            );

            // register commands so the modding API knows about it
			CommandManager.AddCommand(pixelate2DCommand);
			CommandManager.AddCommand(scaleCommand);
			GameEngineManager.AddGameEngine(playerLocationEngine);
            
			GamecraftModdingAPI.Utility.Logging.LogDebug($"{Name} has started up");
		}

        // unused methods

		public void OnFixedUpdate() { } // called once per physics update

		public void OnLevelWasInitialized(int level) { } // called after a level is initialized

		public void OnLevelWasLoaded(int level) { } // called after a level is loaded

		public void OnUpdate() { } // called once per rendered frame (frame update)

        // pixelation methods

        private void pixelate2DFile(string filepath)
		{
            // Load image file and convert to Gamecraft blocks
			Texture2D img = new Texture2D((int)width, (int)height);
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
			float3 position = playerLocationEngine.GetPlayerLocation(0u);
			uint blockCount = 0;
			position.x += 1f;
			//position.y += 1f;
			float zero_y = position.y;
            // convert the image to blocks
            // this groups same-colored pixels in the same column into a single block to reduce the block count
            // any further pixel-grouping optimisations (eg 2D grouping) risk increasing conversion time higher than O(x*y)
			for (int x = 0; x < width; x++)
			{
				QuantizedPixel qVoxel = new QuantizedPixel
				{
					block = BlockIDs.AbsoluteMathsBlock, // impossible canvas block
					color = BlockColors.Default,
					darkness = 10,
					visible = false,
				};
				float3 scale = new float3(1, 1, 1);
				position.x += (float)(blockSize);
				for (int y = 0; y < height; y++)
				{
					//position.y += (float)blockSize;
					Color pixel = img.GetPixel(x, y);
					QuantizedPixel qPixel = quantizeColor(pixel);
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
								Placement.PlaceBlock(qVoxel.block, position, color: qVoxel.color, darkness: qVoxel.darkness, scale: scale);
								blockCount++;
							}
							scale = new float3(1, 1, 1);
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
					position.y = zero_y + (float)((height * blockSize + (height - scale.y) * blockSize) / 2);
					Placement.PlaceBlock(qVoxel.block, position, color: qVoxel.color, darkness: qVoxel.darkness, scale: scale);
					blockCount++;
				}
				//position.y = zero_y;
			}
			Logging.CommandLog($"Placed {width}x{height} image beside you ({blockCount} blocks total)");
			Logging.MetaLog($"Saved {(width * height) - blockCount} blocks ({width * height / blockCount}x) while placing {filepath}");
		}

        private void setScale(uint _width, uint _height)
		{
			width = _width;
			height = _height;
			if (width * height > PIXEL_WARNING_THRESHOLD)
			{
				Logging.CommandLogWarning($"That's a lot of pixels ({width * height}px)!\nImporting large images may freeze your game for a long time.");
			}
			Logging.CommandLog($"Pixi image size set to {width}x{height}");
		}

        private void pixelate3DFile(string filepath)
		{
            // TODO?
			Logging.CommandLogError("Oh no you found this command!\nCommand functionality not implemented (yet)");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private QuantizedPixel quantizeColor(Color pixel)
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

			QuantizedPixel result = new QuantizedPixel
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
	}

	internal struct QuantizedPixel
	{
		public BlockIDs block;

		public BlockColors color;

		public byte darkness;

		public bool visible;
	}
}