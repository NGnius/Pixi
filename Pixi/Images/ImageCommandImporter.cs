using System;
using System.Collections.Generic;
using System.IO;
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
    public class ImageCommandImporter : Importer
    {
        public int Priority { get; } = 0;

        public bool Optimisable { get; } = false;

        public string Name { get; } = "ImageConsole~Spell";

        public BlueprintProvider BlueprintProvider { get; } = null;
        
        private Dictionary<string, string> commandBlockContents = new Dictionary<string, string>();
        
        public bool Qualifies(string name)
        {
            return name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                   || name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase);
        }

        public BlockJsonInfo[] Import(string name)
        {
            // Thanks to Nullpersona for the idea
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
            string text = PixelUtility.TextureToString(img); // conversion
            // save console's command
            commandBlockContents[name] = text;
            return new BlockJsonInfo[]
            {
                new BlockJsonInfo {name = "ConsoleBlock"}
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
            // populate console block
            AsyncUtils.WaitForSubmission(); // just in case
            ConsoleBlock cb = blocks[0].Specialise<ConsoleBlock>();
            cb.Command = "ChangeTextBlockCommand";
            cb.Arg1 = "TextBlockID";
            cb.Arg2 = commandBlockContents[name];
            cb.Arg3 = "";
            commandBlockContents.Remove(name);
        }
    }
}