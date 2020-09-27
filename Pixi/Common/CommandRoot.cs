using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Svelto.ECS;

using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Commands;
using GamecraftModdingAPI.Utility;

namespace Pixi.Common
{
    /// <summary>
    /// Command implementation.
    /// CommandRoot.Pixi is the root of all Pixi calls from the CLI
    /// </summary>
    public class CommandRoot : ICustomCommandEngine
    {
        public void Ready()
        {
            CommandRegistrationHelper.Register<string>(Name, (name) => tryOrCommandLogError(() => this.Pixi(null, name)), Description);
            CommandRegistrationHelper.Register<string, string>(Name+"2", this.Pixi, "Import something into Gamecraft using magic. Usage: Pixi \"importer\" \"myfile.png\"");
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Dispose()
        {
            CommandRegistrationHelper.Unregister(Name);
            CommandRegistrationHelper.Unregister(Name+"2");
        }

        public string Name { get; } = "Pixi";

        public bool isRemovable { get; } = false;

        public string Description { get; } = "Import something into Gamecraft using magic. Usage: Pixi \"myfile.png\"";
        
        public Dictionary<int, Importer[]> importers = new Dictionary<int, Importer[]>();

        public const float BLOCK_SIZE = 0.2f;
        
        public const float DELTA = BLOCK_SIZE / 2048;

        public static int OPTIMISATION_PASSES = 2;

        public CommandRoot()
        {
            CommandManager.AddCommand(this);
        }

        public void Inject(Importer imp)
        {
            if (importers.ContainsKey(imp.Priority))
            {
                // extend array by 1 and place imp at the end 
                Importer[] oldArr = importers[imp.Priority];
                Importer[] newArr = new Importer[oldArr.Length + 1];
                for (int i = 0; i < oldArr.Length; i++)
                {
                    newArr[i] = oldArr[i];
                }
                newArr[oldArr.Length] = imp;
                importers[imp.Priority] = newArr;
            }
            else
            {
                importers[imp.Priority] = new Importer[] {imp};
            }
        }

        private void Pixi(string importerName, string name)
        {
            // organise priorities
            int[] priorities = importers.Keys.ToArray();
            Array.Sort(priorities);
            Array.Reverse(priorities); // higher priorities go first
            // find relevant importer
            Importer magicImporter = null;
            foreach (int p in priorities)
            {
                Importer[] imps = importers[p];
                for (int i = 0; i < imps.Length; i++)
                {
                    //Logging.MetaLog($"Now checking importer {imps[i].Name}");
                    if ((importerName == null && imps[i].Qualifies(name))
                        || (importerName != null && imps[i].Name.Contains(importerName)))
                    {
                        magicImporter = imps[i];
                        break;
                    }
                }
                if (magicImporter != null) break;
            }

            if (magicImporter == null)
            {
                Logging.CommandLogError("Unsupported file or string.");
                return;
            }
#if DEBUG
            Logging.MetaLog($"Using '{magicImporter.Name}' to import '{name}'");
#endif
            // import blocks
            BlockJsonInfo[] blocksInfo = magicImporter.Import(name);
            if (blocksInfo.Length == 0)
            {
#if DEBUG
                Logging.CommandLogError($"Importer {magicImporter.Name} didn't provide any blocks to import. Mission Aborted!");
#endif
                return;
            }

            ProcessedVoxelObjectNotation[][] procVONs;
            BlueprintProvider blueprintProvider = magicImporter.BlueprintProvider;
            if (blueprintProvider == null)
            {
                // convert block info to API-compatible format
                procVONs = new ProcessedVoxelObjectNotation[][] {BlueprintUtility.ProcessBlocks(blocksInfo)};
            }
            else
            {
                // expand blueprints and convert block info
                procVONs = BlueprintUtility.ProcessAndExpandBlocks(name, blocksInfo, magicImporter.BlueprintProvider);
            }
            // reduce block placements by grouping neighbouring similar blocks
            // (after flattening block data representation)
            List<ProcessedVoxelObjectNotation> optVONs = new List<ProcessedVoxelObjectNotation>();
            for (int arr = 0; arr < procVONs.Length; arr++)
            {
                for (int elem = 0; elem < procVONs[arr].Length; elem++)
                {
                    optVONs.Add(procVONs[arr][elem]);
                }
            }
#if DEBUG
            Logging.MetaLog($"Imported {optVONs.Count} blocks for '{name}'");
#endif
            int blockCountPreOptimisation = optVONs.Count;
            if (magicImporter.Optimisable)
            {
                for (int pass = 0; pass < OPTIMISATION_PASSES; pass++)
                {
                    OptimiseBlocks(ref optVONs);
#if DEBUG
                    Logging.MetaLog($"Optimisation pass {pass} completed");
#endif
                }
#if DEBUG
                Logging.MetaLog($"Optimised down to {optVONs.Count} blocks for '{name}'");
#endif
            }
            ProcessedVoxelObjectNotation[] optVONsArr = optVONs.ToArray();
            magicImporter.PreProcess(name, ref optVONsArr);
            // place blocks
            Block[] blocks = new Block[optVONsArr.Length];
            for (int i = 0; i < optVONsArr.Length; i++)
            {
                ProcessedVoxelObjectNotation desc = optVONsArr[i];
                Block b;
                if (desc.block != BlockIDs.Invalid)
                {
                    b = Block.PlaceNew(desc.block, desc.position, desc.rotation, desc.color.Color,
                        desc.color.Darkness, 1, desc.scale);
                }
                else
                {
                    TextBlock tb = Block.PlaceNew<TextBlock>(BlockIDs.TextBlock, desc.position, desc.rotation,
                        desc.color.Color, desc.color.Darkness, 1, desc.scale);
                    tb.Text = desc.metadata;
                    b = tb;
                }

                blocks[i] = b;
            }
            magicImporter.PostProcess(name, ref blocks);
            if (magicImporter.Optimisable && blockCountPreOptimisation > blocks.Length)
            {
                Logging.CommandLog($"Imported {blocks.Length} blocks using {magicImporter.Name} ({blockCountPreOptimisation/blocks.Length}x ratio)");
            }
            else
            {
                Logging.CommandLog($"Imported {blocks.Length} blocks using {magicImporter.Name}");
            }
            
        }

        private void OptimiseBlocks(ref List<ProcessedVoxelObjectNotation> optVONs)
        {
            // a really complicated algorithm to determine if two similar blocks are touching (before they're placed)
            // the general concept:
            // two blocks are touching when they have a common face (equal to 4 corners on the cube, where the 4 corners aren't completely opposite each other)
            // between the two blocks, the 8 corners that aren't in common are the corners for the merged block
            //
            // to merge the 2 blocks, switch out the 4 common corners of one block with the nearest non-common corners from the other block
            // i.e. swap the common face on block A with the face opposite the common face of block B
            // to prevent a nonsensical face (rotated compared to other faces), the corners of the face should be swapped out with the corresponding corner which shares an edge
            //
            // note: e.g. if common face on block A is its top, the common face of block B is not necessarily the bottom face because blocks can be rotated differently
            // this means it's not safe to assume that block A's common face (top) can be swapped with block B's non-common opposite face (top) to get the merged block
            //
            // note2: this does not work with blocks which aren't cubes (i.e. any block where rotation matters)
            // TODO multithread this expensive operation
            int item = 0;
            while (item < optVONs.Count)
            {
                bool isItemUpdated = false;
                ProcessedVoxelObjectNotation itemVON = optVONs[item];
                if (isOptimisableBlock(itemVON.block))
                {
                    float3[] itemCorners = calculateCorners(itemVON);
                    int seeker = item + 1; // despite this, assume that seeker goes thru the entire list (not just blocks after item)
                    while (seeker < optVONs.Count)
                    {
                        if (seeker == item)
                        {
                            seeker++;
                        }
                        else
                        {
                            ProcessedVoxelObjectNotation seekerVON = optVONs[seeker];
                            //Logging.MetaLog($"Comparing {itemVON} and {seekerVON}");
                            float3[] seekerCorners = calculateCorners(seekerVON);
                            int[][] mapping = findMatchingCorners(itemCorners, seekerCorners);
                            if (mapping.Length != 0
                                && itemVON.block == seekerVON.block
                                && itemVON.color.Color == seekerVON.color.Color
                                && itemVON.color.Darkness == seekerVON.color.Darkness
                                && isOptimisableBlock(seekerVON.block)) // match found
                            {
                                // switch out corners based on mapping
                                //Logging.MetaLog($"Corners {float3ArrToString(itemCorners)}\nand {float3ArrToString(seekerCorners)}");
                                //Logging.MetaLog($"Mappings (len:{mapping[0].Length}) {mapping[0][0]} -> {mapping[1][0]}\n{mapping[0][1]} -> {mapping[1][1]}\n{mapping[0][2]} -> {mapping[1][2]}\n{mapping[0][3]} -> {mapping[1][3]}\n");
                                for (byte i = 0; i < 4; i++)
                                {
                                    itemCorners[mapping[0][i]] = seekerCorners[mapping[1][i]];
                                }
                                // remove 2nd block, since it's now part of the 1st block
                                //Logging.MetaLog($"Removing {seekerVON}");
                                optVONs.RemoveAt(seeker);
                                if (seeker < item)
                                {
                                    item--; // note: this will never become less than 0
                                }
                                isItemUpdated = true;
                                // regenerate info
                                //Logging.MetaLog($"Final corners {float3ArrToString(itemCorners)}");
                                updateVonFromCorners(itemCorners, ref itemVON);
                                itemCorners = calculateCorners(itemVON);
                                //Logging.MetaLog($"Merged block is {itemVON}");
                            }
                            else
                            {
                                seeker++;
                            }
                        }
                    }

                    if (isItemUpdated)
                    {
                        optVONs[item] = itemVON;
                        //Logging.MetaLog($"Optimised block is now {itemVON}");
                    }
                    item++;
                }
                else
                {
                    item++;
                }
            }
        }

        private float3[] calculateCorners(ProcessedVoxelObjectNotation von)
        {
            float3[] cornerMultiplicands = new float3[8]
            {
                new float3(1, 1, 1),
                new float3(1, 1, -1),
                new float3(-1, 1, 1),
                new float3(-1, 1, -1),
                new float3(-1, -1, 1),
                new float3(-1, -1, -1),
                new float3(1, -1, 1),
                new float3(1, -1, -1),
            };
            float3[] corners = new float3[8];
            Quaternion rotation = Quaternion.Euler(von.rotation);
            float3 rotatedScale = rotation * von.scale;
            float3 trueCenter = von.position;
            // generate corners
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = trueCenter + BLOCK_SIZE * (cornerMultiplicands[i] * rotatedScale / 2);
            }
            return corners;
        }

        private void updateVonFromCorners(float3[] corners, ref ProcessedVoxelObjectNotation von)
        {
            float3[] cornerMultiplicands = new float3[8]
            {
                new float3(1, 1, 1),
                new float3(1, 1, -1),
                new float3(1, -1, 1),
                new float3(1, -1, -1),
                new float3(-1, 1, 1),
                new float3(-1, 1, -1),
                new float3(-1, -1, 1),
                new float3(-1, -1, -1),
            };
            float3 newCenter = sumOfFloat3Arr(corners) / corners.Length;
            float3 newPosition = newCenter;
            Quaternion rot = Quaternion.Euler(von.rotation);
            float3 rotatedScale = 2 * (corners[0] - newCenter) / BLOCK_SIZE;
            von.scale = Quaternion.Inverse(rot) * rotatedScale;
            von.position = newPosition;
            //Logging.MetaLog($"Updated VON scale {von.scale} (absolute {rotatedScale})");
        }
        
        private int[][] findMatchingCorners(float3[] corners1, float3[] corners2)
        {
            int[][] cornerFaceMappings = new int[][]
            {
                new int[] {0, 1, 2, 3}, // top
                new int[] {2, 3, 4, 5}, // left
                new int[] {4, 5, 6, 7}, // bottom
                new int[] {6, 7, 0, 1}, // right
                new int[] {0, 2, 4, 6}, // back
                new int[] {1, 3, 5, 7}, // front
            };
            int[][] oppositeFaceMappings = new int[][]
            {
                new int[] {6, 7, 4, 5}, // bottom
                new int[] {0, 1, 6, 7}, // right
                new int[] {2, 3, 0, 1}, // top
                new int[] {4, 5, 2, 3}, // left
                new int[] {1, 3, 5, 7}, // front
                new int[] {0, 2, 4, 6}, // back
            };
            float3[][] faces1 = facesFromCorners(corners1);
            float3[][] faces2 = facesFromCorners(corners2);
            for (byte i = 0; i < faces1.Length; i++)
            {
                for (byte j = 0; j < faces2.Length; j++)
                {
                    //Logging.MetaLog($"Checking faces {float3ArrToString(faces1[i])} and {float3ArrToString(faces2[j])}");
                    int[] match = matchFace(faces1[i], faces2[j]);
                    if (match.Length != 0)
                    {
                        //Logging.MetaLog($"Matched faces {float3ArrToString(faces1[i])} and {float3ArrToString(faces2[j])}");
                        // translate from face mapping to corner mapping
                        for (byte k = 0; k < match.Length; k++)
                        {
                            match[k] = oppositeFaceMappings[j][match[k]];
                        }
                        return new int[][] {cornerFaceMappings[i], match}; // {{itemCorners index}, {seekerCorners index}}
                    }
                }
            }
            return new int[0][];
        }
        
        // this assumes the corners are in the order that calculateCorners outputs
        private float3[][] facesFromCorners(float3[] corners)
        {
            return new float3[][]
            {
                new float3[] {corners[0], corners[1], corners[2], corners[3]}, // top
                new float3[] {corners[2], corners[3], corners[4], corners[5]}, // left
                new float3[] {corners[4], corners[5], corners[6], corners[7]}, // bottom
                new float3[] {corners[6], corners[7], corners[0], corners[1]}, // right
                new float3[] {corners[0], corners[2], corners[4], corners[6]}, // back
                new float3[] {corners[1], corners[3], corners[5], corners[7]}, // front
            };
        }

        private int[] matchFace(float3[] face1, float3[] face2)
        {
            int[] result = new int[4];
            byte count = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    //Logging.MetaLog($"Comparing {face1[i]} and {face1[i]} ({Mathf.Abs(face1[i].x - face2[j].x)} & {Mathf.Abs(face1[i].y - face2[j].y)} & {Mathf.Abs(face1[i].z - face2[j].z)} vs {DELTA})");
                    // if (face1[i] == face2[j])
                    if (Mathf.Abs(face1[i].x - face2[j].x) < DELTA
                        && Mathf.Abs(face1[i].y - face2[j].y) < DELTA
                        && Mathf.Abs(face1[i].z - face2[j].z) < DELTA)
                    {
                        count++;
                        result[i] = j; // map corners to each other
                        break;
                    }
                }
            }
            //Logging.MetaLog($"matched {count}/4");
            if (count == 4)
            {
                return result;
            }
            return new int[0];
        }

        private float3 sumOfFloat3Arr(float3[] arr)
        {
            float3 total = float3.zero;
            for (int i = 0; i < arr.Length; i++)
            {
                total += arr[i];
            }

            return total;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isOptimisableBlock(BlockIDs block)
        {
            return block.ToString().EndsWith("Cube", StringComparison.InvariantCultureIgnoreCase);
        }

        private string float3ArrToString(float3[] arr)
        {
            string result = "[";
            foreach (float3 f in arr)
            {
                result += f.ToString() + ", ";
            }

            return result.Substring(0, result.Length - 2) + "]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void tryOrCommandLogError(Action toTry)
        {
            try
            {
                toTry();
            }
            catch (Exception e)
            {
#if DEBUG
                    Logging.CommandLogError("RIP\n" + e);
#else
                    Logging.CommandLogError("Pixi failed (reason: " + e.Message + ")");
#endif
            }
        }
    }
}