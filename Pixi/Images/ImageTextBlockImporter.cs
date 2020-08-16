using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Players;
using GamecraftModdingAPI.Utility;
using Pixi.Common;

namespace Pixi.Images
{
    public class ImageTextBlockImporter : Importer
    {
        public int Priority { get; } = 0;
        
        public bool Optimisable { get; } = false;
        
        public string Name { get; } = "ImageText~Spell";

        public BlueprintProvider BlueprintProvider { get; } = null;
        
        private Dictionary<string, string[]> textBlockContents = new Dictionary<string, string[]>();
        
        public bool Qualifies(string name)
        {
            return name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                   || name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase);
        }

        public BlockJsonInfo[] Import(string name)
        {
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
            string text = PixelUtility.TextureToString(img);
            // generate text block name
            byte[] textHash;
            using (HashAlgorithm hasher = SHA256.Create())
                textHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(text));
            string textId = "Pixi_";
            for (int i = 0; i < 2 && i < textHash.Length; i++)
            {
                textId += textHash[i].ToString("X2");
            }
            
            // save text block info for post-processing
            textBlockContents[name] = new string[2] { textId, text};
            
            return new BlockJsonInfo[1]
            {
                new BlockJsonInfo
                {
                    color = new float[] {-1f, -1f, -1f},
                    name = "TextBlock",
                    position = new float[] {0f, 0f, 0f},
                    rotation = new float[] {0f, 0f, 0f},
                    scale = new float[] {Mathf.Ceil(img.width / 16f), 1f, Mathf.Ceil(img.height / 16f)}
                }
            };
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

        public void PostProcess(string name, ref Block[] blocks)
        {
            // populate text block
            AsyncUtils.WaitForSubmission(); // just in case
            TextBlock tb = blocks[0].Specialise<TextBlock>();
            tb.TextBlockId = textBlockContents[name][0];
            tb.Text = textBlockContents[name][1];
            textBlockContents.Remove(name);
        }
    }
}