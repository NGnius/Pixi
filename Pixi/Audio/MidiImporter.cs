using System;
using System.Collections.Generic;
using System.Linq;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Players;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Utility;
using Melanchall.DryWetMidi.Common;
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

        public static byte Key = 0;

        public MidiImporter()
        {
            AudioTools.GenerateProgramMap();
        }
        
        public bool Qualifies(string name)
        {
            return name.EndsWith(".mid", StringComparison.InvariantCultureIgnoreCase)
                   || name.EndsWith(".midi", StringComparison.InvariantCultureIgnoreCase);
        }

        public BlockJsonInfo[] Import(string name)
        {
            MidiFile midi = MidiFile.Read(name);
            openFiles[name] = midi;
            Logging.MetaLog($"Found {midi.GetNotes().Count()} notes over {midi.GetDuration<MidiTimeSpan>().TimeSpan} time units");
            BlockJsonInfo[] blocks = new BlockJsonInfo[(midi.GetNotes().Count() * 2) + 3];
            List<BlockJsonInfo> blocksToBuild = new List<BlockJsonInfo>();
#if DEBUG
            // test (for faster, but incomplete, imports)
            if (blocks.Length > 103) blocks = new BlockJsonInfo[103];
#endif
            // convert Midi notes to sfx blocks
            Dictionary<long, uint> breadthCache = new Dictionary<long, uint>();
            Dictionary<long, uint> depthCache = new Dictionary<long, uint>();
            HashSet<long> timerCache = new HashSet<long>();
            //uint count = 0;
            float zdepth = 0;
            foreach (Note n in midi.GetNotes())
            {
                long microTime = n.TimeAs<MetricTimeSpan>(midi.GetTempoMap()).TotalMicroseconds;
                float breadth = 0f;
                if (!timerCache.Contains(microTime))
                {
                    depthCache[microTime] = (uint)++zdepth;
                    breadthCache[microTime] = 1;
                    timerCache.Add(microTime);
                    blocksToBuild.Add(new BlockJsonInfo
                    {
                        name = GamecraftModdingAPI.Blocks.BlockIDs.Timer.ToString(),
                        position = new float[] { breadth * 0.2f * Spread, 2 * 0.2f, zdepth * 0.2f * Spread},
                        rotation = new float[] { 0, 0, 0},
                        color = new float[] { -1, -1, -1},
                        scale = new float[] { 1, 1, 1},
                    });
                }
                else
                {
                    zdepth = depthCache[microTime]; // remember the z-position of notes played at the same moment (so they can be placed adjacent to each other)
                    breadth += breadthCache[microTime]++; // if multiple notes exist for a given time, place them beside each other on the x-axis
                }
                blocksToBuild.Add(new BlockJsonInfo
                {
                    name = GamecraftModdingAPI.Blocks.BlockIDs.SFXBlockInstrument.ToString(),
                    position = new float[] { breadth * 0.2f * Spread, 1 * 0.2f, zdepth * 0.2f * Spread},
                    rotation = new float[] { 0, 0, 0},
                    color = new float[] { -1, -1, -1},
                    scale = new float[] { 1, 1, 1},
                });
                /*
                blocks[count] = new BlockJsonInfo
                {
                    name = GamecraftModdingAPI.Blocks.BlockIDs.Timer.ToString(),
                    position = new float[] { breadth * 0.2f * Spread, 2 * 0.2f, zdepth * 0.2f * Spread},
                    rotation = new float[] { 0, 0, 0},
                    color = new float[] { -1, -1, -1},
                    scale = new float[] { 1, 1, 1},
                };
                count++;
                blocks[count] = new BlockJsonInfo
                {
                    name = GamecraftModdingAPI.Blocks.BlockIDs.SFXBlockInstrument.ToString(),
                    position = new float[] { breadth * 0.2f * Spread, 1 * 0.2f, zdepth * 0.2f * Spread},
                    rotation = new float[] { 0, 0, 0},
                    color = new float[] { -1, -1, -1},
                    scale = new float[] { 1, 1, 1},
                };
                count++;*/
            }
            // playback IO (reset & play)
            blocksToBuild.Add(new BlockJsonInfo
            {
                name = GamecraftModdingAPI.Blocks.BlockIDs.SimpleConnector.ToString(),
                position = new float[] { -0.2f, 3 * 0.2f, 0},
                rotation = new float[] { 0, 0, 0},
                color = new float[] { -1, -1, -1},
                scale = new float[] { 1, 1, 1},
            }); // play is second last (placed above stop)
            blocksToBuild.Add(new BlockJsonInfo
            {
                name = GamecraftModdingAPI.Blocks.BlockIDs.SimpleConnector.ToString(),
                position = new float[] { -0.2f, 2 * 0.2f, 0},
                rotation = new float[] { 0, 0, 0},
                color = new float[] { -1, -1, -1},
                scale = new float[] { 1, 1, 1},
            }); // stop is middle (placed above reset)
            blocksToBuild.Add(new BlockJsonInfo
            {
                name = GamecraftModdingAPI.Blocks.BlockIDs.SimpleConnector.ToString(),
                position = new float[] { -0.2f, 1 * 0.2f, 0},
                rotation = new float[] { 0, 0, 0},
                color = new float[] { -1, -1, -1},
                scale = new float[] { 1, 1, 1},
            }); // reset is last (placed below stop)
            return blocksToBuild.ToArray();
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
            LogicGate startConnector = blocks[blocks.Length - 3].Specialise<LogicGate>();
            LogicGate stopConnector = blocks[blocks.Length - 2].Specialise<LogicGate>();
            LogicGate resetConnector = blocks[blocks.Length - 1].Specialise<LogicGate>();
            uint count = 0;
            // generate channel data
            byte[] channelPrograms = new byte[16];
            for (byte i = 0; i < channelPrograms.Length; i++) // init array
            {
                channelPrograms[i] = 5; // Piano
            }

            foreach (TimedEvent e in openFiles[name].GetTimedEvents())
            {
                if (e.Event.EventType == MidiEventType.ProgramChange)
                {
                    ProgramChangeEvent pce = (ProgramChangeEvent) e.Event;
                    channelPrograms[pce.Channel] = AudioTools.TrackType(pce.ProgramNumber);
#if DEBUG
                    Logging.MetaLog($"Detected channel {pce.Channel} as program {pce.ProgramNumber} (index {channelPrograms[pce.Channel]})");
#endif
                }
            }

            Timer t = null;
            //count = 0;
            foreach (Note n in openFiles[name].GetNotes())
            {
                while (blocks[count].Type == BlockIDs.Timer)
                {
                    // set timing info
#if DEBUG
                    Logging.Log($"Handling Timer for notes at {n.TimeAs<MetricTimeSpan>(openFiles[name].GetTempoMap()).TotalMicroseconds * 0.000001f}s");
#endif
                    t = blocks[count].Specialise<Timer>();
                    t.Start = 0;
                    t.End = 0.01f + n.TimeAs<MetricTimeSpan>(openFiles[name].GetTempoMap()).TotalMicroseconds * 0.000001f;
                    count++;
                }
                // set notes info
                SfxBlock sfx = blocks[count].Specialise<SfxBlock>();
                sfx.Pitch = n.NoteNumber - 60 + Key; // In MIDI, 60 is middle C, but GC uses 0 for middle C
                sfx.TrackIndex = channelPrograms[n.Channel];
                sfx.Is3D = ThreeDee;
                sfx.Volume = AudioTools.VelocityToVolume(n.Velocity);
                count++;
                // connect wires
                if (t == null) continue; // this should never happen
                t.Connect(0, sfx, 0);
                startConnector.Connect(0, t, 0);
                stopConnector.Connect(0, t, 1);
                resetConnector.Connect(0, t, 2);
            }
            openFiles.Remove(name);
        }
    }
}