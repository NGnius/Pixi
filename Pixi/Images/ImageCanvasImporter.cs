using System;
using System.Collections.Generic;
using System.IO;

using Unity.Mathematics;
using UnityEngine;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Players;
using GamecraftModdingAPI.Utility;
using Pixi.Common;

namespace Pixi.Images
{
    public class ImageCanvasImporter : Importer
    {
        public static float3 Rotation = float3.zero;

        public static uint Thiccness = 1;
            
        public int Priority { get; } = 1;

        public bool Optimisable { get; } = true;

        public string Name { get; } = "ImageCanvas~Spell";

        public BlueprintProvider BlueprintProvider { get; } = null;

        public ImageCanvasImporter()
        {
            GamecraftModdingAPI.App.Client.EnterMenu += ColorSpaceUtility.LoadColorMenuEvent;
        }
        
        public bool Qualifies(string name)
        {
            //Logging.MetaLog($"Qualifies received name {name}");
            return name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                   || name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase);
        }

        public BlockJsonInfo[] Import(string name)
        {
            // Load image file and convert to Gamecraft blocks
            Texture2D img = new Texture2D(64, 64);
            // load file into texture
            try
            {
                byte[] imgData = File.ReadAllBytes(name);
                img.LoadImage(imgData);
            }
            catch (Exception e)
            {
                Logging.CommandLogError($"Failed to load picture data. Reason: {e.Message}");
                Logging.MetaLog(e.Message + "\n" + e.StackTrace);
                return new BlockJsonInfo[0];
            }
			//Logging.CommandLog($"Image size: {img.width}x{img.height}");
            Player p = new Player(PlayerType.Local);
            string pickedBlock = p.SelectedBlock == BlockIDs.Invalid ? BlockIDs.AluminiumCube.ToString() : p.SelectedBlock.ToString();
            Quaternion imgRotation = Quaternion.Euler(Rotation);

            BlockJsonInfo[] blocks = new BlockJsonInfo[img.width * img.height];
            // convert the image to blocks
            // optimisation occurs later
            for (int x = 0; x < img.width; x++)
            {
                for (int y = 0; y < img.height; y++)
                {
                    Color pixel = img.GetPixel(x, y);
                    float3 position = (imgRotation * (new float3((x * CommandRoot.BLOCK_SIZE),y * CommandRoot.BLOCK_SIZE,0)));
                    BlockJsonInfo qPixel = new BlockJsonInfo
                    {
                        name = pixel.a > 0.75 ? pickedBlock : BlockIDs.GlassCube.ToString(),
                        color = new float[] {pixel.r, pixel.g, pixel.b},
                        rotation = ConversionUtility.Float3ToFloatArray(Rotation),
                        position = ConversionUtility.Float3ToFloatArray(position),
                        scale = new float[] { 1, 1, Thiccness},
                    };
                    if (pixel.a < 0.5f) qPixel.name = BlockIDs.Invalid.ToString();
                    blocks[(x * img.height) + y] = qPixel;
                }
            }
            return blocks;
        }

        public void PreProcess(string name, ref ProcessedVoxelObjectNotation[] blocks)
        {
            Player p = new Player(PlayerType.Local);
            float3 pos = p.Position;
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].position += pos;
            }
        }

        public void PostProcess(string name, ref Block[] blocks) { }
    }
}