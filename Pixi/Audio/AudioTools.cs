using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GamecraftModdingAPI.Utility;
using Melanchall.DryWetMidi.Common;

namespace Pixi.Audio
{
    public static class AudioTools
    {
        private static Dictionary<byte, byte> programMap = null;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte TrackType(FourBitNumber channel)
        {
            return TrackType((byte) channel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte TrackType(byte channel)
        {
            if (programMap.ContainsKey(channel)) return programMap[channel];
#if DEBUG
            Logging.MetaLog($"Using default value (piano) for channel number {channel}");
#endif
            return 5; // Piano
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float VelocityToVolume(SevenBitNumber velocity)
        {
            // faster key hit means louder note
            return 100f * velocity / ((float) SevenBitNumber.MaxValue + 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GenerateProgramMap()
        {
            programMap = new Dictionary<byte, byte>
            {
                {0, 5 /* Piano */},
                {1, 5},
                {2, 5},
                {3, 5},
                {4, 5},
                {5, 5},
                {6, 5},
                {7, 5},
                {8, 0 /* Kick Drum */},
                {9, 0},
                {10, 0},
                {11, 0},
                {12, 0},
                {13, 0},
                {14, 0},
                {15, 0},
                {24, 6 /* Guitar 1 (Acoustic) */},
                {25, 6},
                {26, 6},
                {27, 6},
                {28, 6},
                {29, 7 /* Guitar 2 (Dirty Electric) */},
                {30, 7},
                {32, 6},
                {33, 6},
                {34, 6},
                {35, 6},
                {36, 6},
                {37, 6},
                {38, 6},
                {39, 6},
                {56, 8 /* Trumpet */}, // basically all brass & reeds are trumpets... that's how music works right?
                {57, 8},
                {58, 8},
                {59, 8},
                {60, 8},
                {61, 8},
                {62, 8},
                {63, 8},
                {64, 8},
                {65, 8},
                {66, 8},
                {67, 8},
                {68, 8},
                {69, 8}, // Nice
                {70, 8},
                {71, 8},
                {72, 8},
                {73, 8},
                {74, 8},
                {75, 8},
                {76, 8},
                {77, 8},
                {78, 8},
                {79, 8},
                {112, 0},
                {113, 0},
                {114, 0},
                {115, 0},
                {116, 0},
                {117, 4 /* Tom Drum */},
                {118, 4},
                {119, 3 /* Open High Hat */},
            };
        }
    }
}