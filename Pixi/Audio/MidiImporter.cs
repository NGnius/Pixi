using System;
using System.Collections.Generic;
using System.Linq;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Players;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Utility;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;

using Pixi.Common;
using Unity.Mathematics;

namespace Pixi.Audio
{
    public class MidiImporter : Importer
    {
        public int Priority { get; } = 1;
        public bool Optimisable { get; } = false;
        public string Name { get; } = "Midi~Spell";
        public BlueprintProvider BlueprintProvider { get; } = null;
        
        private Dictionary<string, MidiFile> openFiles = new Dictionary<string, MidiFile>();

        public static bool ThreeDee = false;

        public static float Spread = 1f;
        
        public bool Qualifies(string name)
        {
            return name.EndsWith(".mid", StringComparison.InvariantCultureIgnoreCase);
        }

        public BlockJsonInfo[] Import(string name)
        {
            MidiFile midi = MidiFile.Read(name);
            openFiles[name] = midi;
            Logging.MetaLog($"Found {midi.GetNotes().Count()} notes over {midi.GetDuration<MidiTimeSpan>().TimeSpan} time units");
            BlockJsonInfo[] blocks = new BlockJsonInfo[(midi.GetNotes().Count() * 2) + 2];
#if DEBUG
            // test (for faster, but incomplete, imports)
            if (blocks.Length > 102) blocks = new BlockJsonInfo[102];
#endif
            // convert Midi notes to sfx blocks
            Dictionary<long, uint> breadthCache = new Dictionary<long, uint>();
            uint count = 0;
            foreach (Note n in midi.GetNotes())
            {
                // even blocks are counters,
                long microTime = n.TimeAs<MetricTimeSpan>(midi.GetTempoMap()).TotalMicroseconds;
                float breadth = 1f;
                if (breadthCache.ContainsKey(microTime))
                {
                    breadth += breadthCache[microTime]++;
                }
                else
                {
                    breadthCache[microTime] = 1;
                }
                blocks[count] = new BlockJsonInfo
                {
                    name = GamecraftModdingAPI.Blocks.BlockIDs.Timer.ToString(),
                    position = new float[] { breadth * 0.2f * Spread, 2 * 0.2f, microTime * 0.00001f * 0.2f * Spread},
                    rotation = new float[] { 0, 0, 0},
                    color = new float[] { -1, -1, -1},
                    scale = new float[] { 1, 1, 1},
                };
                count++;
                blocks[count] = new BlockJsonInfo
                {
                    name = GamecraftModdingAPI.Blocks.BlockIDs.SFXBlockInstrument.ToString(),
                    position = new float[] { breadth * 0.2f * Spread, 1 * 0.2f, microTime * 0.00001f * 0.2f * Spread},
                    rotation = new float[] { 0, 0, 0},
                    color = new float[] { -1, -1, -1},
                    scale = new float[] { 1, 1, 1},
                };
                count++;
#if DEBUG
                // test (for faster, but incomplete, imports)
                if (count >= 100) break;
#endif
            }
            // playback IO (reset & play)
            blocks[count] = new BlockJsonInfo
            {
                name = GamecraftModdingAPI.Blocks.BlockIDs.SimpleConnector.ToString(),
                position = new float[] { -0.2f, 2 * 0.2f, 0},
                rotation = new float[] { 0, 0, 0},
                color = new float[] { -1, -1, -1},
                scale = new float[] { 1, 1, 1},
            }; // play is second last (placed above reset)
            count++;
            blocks[count] = new BlockJsonInfo
            {
                name = GamecraftModdingAPI.Blocks.BlockIDs.SimpleConnector.ToString(),
                position = new float[] { -0.2f, 1 * 0.2f, 0},
                rotation = new float[] { 0, 0, 0},
                color = new float[] { -1, -1, -1},
                scale = new float[] { 1, 1, 1},
            }; // reset is last (placed below play)
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

        public void PostProcess(string name, ref Block[] blocks)
        {
            // playback IO
            LogicGate startConnector = blocks[blocks.Length - 2].Specialise<LogicGate>();
            LogicGate resetConnector = blocks[blocks.Length - 1].Specialise<LogicGate>();
            uint count = 0;
            foreach (Note n in openFiles[name].GetNotes())
            {
                // set timing info
                Timer t = blocks[count].Specialise<Timer>();
                t.Start = 0;
                t.End = n.TimeAs<MetricTimeSpan>(openFiles[name].GetTempoMap()).TotalMicroseconds * 0.000001f;
                count++;
                // set notes info
                SfxBlock sfx = blocks[count].Specialise<SfxBlock>();
                sfx.Pitch = n.NoteNumber - 60; // In MIDI, 60 is middle C, but GC uses 0 for middle C
                sfx.TrackIndex = 5; // Piano
                sfx.Is3D = ThreeDee;
                count++;
                // connect wires
                t.Connect(0, sfx, 0);
                startConnector.Connect(0, t, 0);
                resetConnector.Connect(0, t, 2);
#if DEBUG
                // test (for faster, but incomplete, imports)
                if (count >= 100) break;
#endif
            }
            openFiles.Remove(name);
        }
    }
}