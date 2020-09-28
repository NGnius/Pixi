using System;
using GamecraftModdingAPI;
using GamecraftModdingAPI.Utility;
using Pixi.Common;

namespace Pixi.Audio
{
    public class AudioFakeImporter : Importer
    {
        public int Priority { get; } = 0;
        public bool Optimisable { get; } = false;
        public string Name { get; } = "AudioWarning~Spell";
        public BlueprintProvider BlueprintProvider { get; } = null;
        public bool Qualifies(string name)
        {
            return name.EndsWith(".flac", StringComparison.InvariantCultureIgnoreCase)
                || name.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase)
                || name.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)
                || name.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase)
                || name.EndsWith(".aac", StringComparison.InvariantCultureIgnoreCase);
        }

        public BlockJsonInfo[] Import(string name)
        {
            Logging.CommandLogWarning($"Audio importing only works with MIDI (.mid) files, which '{name}' is not.\nThere are many converters online, but for best quality use a MIDI file made from a music transcription.\nFor example, musescore.com has lots of good transcriptions and they offer a 30-day free trial.");
            return null;
        }

        public void PreProcess(string name, ref ProcessedVoxelObjectNotation[] blocks)
        {
        }

        public void PostProcess(string name, ref Block[] blocks)
        {
        }
    }
}