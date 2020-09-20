using System;
using System.Collections.Generic;

using Svelto.DataStructures;
using Unity.Mathematics;
using UnityEngine;

using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Commands;
using GamecraftModdingAPI.Utility;
using Pixi.Common;

namespace Pixi.Robots
{
    public class RobotBlueprintProvider : BlueprintProvider
    {
        public string Name { get; } = "RobotBlueprintProvider";

        private Dictionary<string, BlockJsonInfo[]> botprints = null;

        private RobotInternetImporter parent;

        public RobotBlueprintProvider(RobotInternetImporter rii)
        {
            parent = rii;
        }
        
        public BlockJsonInfo[] Blueprint(string name, BlockJsonInfo root)
        {
            if (botprints == null)
            {
                botprints = BlueprintUtility.ParseBlueprintResource("Pixi.blueprints.json");
            }

            if (!botprints.ContainsKey(root.name) || RobotInternetImporter.CubeSize != 3)
            {
                if (!parent.textBlockInfo.ContainsKey(name))
                {
                    parent.textBlockInfo[name] = new FasterList<string>();
                }
                BlockJsonInfo copy = root;
                copy.name = "TextBlock";
                Logging.MetaLog($"Parsing uint from '{root.name}'");
                parent.textBlockInfo[name].Add(root.name + " (" + CubeUtility.CubeIdDescription(uint.Parse(root.name)) + ")");
                return new BlockJsonInfo[1] {copy};
            }
            BlockJsonInfo[] blueprint = botprints[root.name];
            BlockJsonInfo[] adjustedBlueprint = new BlockJsonInfo[blueprint.Length];
            Quaternion cubeQuaternion = Quaternion.Euler(ConversionUtility.FloatArrayToFloat3(root.rotation));
            if (blueprint.Length == 0)
            {
                Logging.LogWarning($"Found empty blueprint for {root.name} (during '{name}'), is the blueprint correct?");
                return new BlockJsonInfo[0];
            }
            // move blocks to correct position & rotation
            float3 defaultCorrectionVec = new float3((float)(0), (float)(CommandRoot.BLOCK_SIZE), (float)(0));
			float3 baseRot = new float3(blueprint[0].rotation[0], blueprint[0].rotation[1], blueprint[0].rotation[2]);
			float3 baseScale = new float3(blueprint[0].scale[0], blueprint[0].scale[1], blueprint[0].scale[2]);
			//Block[] placedBlocks = new Block[blueprint.Length];
			bool isBaseScaled = !(blueprint[0].scale[1] > 0f && blueprint[0].scale[1] < 2f);
			float3 correctionVec = isBaseScaled ? (float3)(Quaternion.Euler(baseRot) * baseScale / 2) * (float)-CommandRoot.BLOCK_SIZE : -defaultCorrectionVec;
            // FIXME scaled base blocks cause the blueprint to be placed in the wrong location (this also could be caused by a bug in DumpVON command)
			if (isBaseScaled)
			{
				Logging.LogWarning($"Found blueprint with scaled base block for {root.name} (during '{name}'), this is not currently supported");
			}

			float3 rootPos = ConversionUtility.FloatArrayToFloat3(root.position);
			for (int i = 0; i < blueprint.Length; i++)
			{
				BlockColor blueprintBlockColor = ColorSpaceUtility.QuantizeToBlockColor(blueprint[i].color);
				float[] physicalColor = blueprintBlockColor.Color == BlockColors.White && blueprintBlockColor.Darkness == 0 ? root.color : blueprint[i].color;
				float3 bluePos = ConversionUtility.FloatArrayToFloat3(blueprint[i].position);
				float3 blueScale = ConversionUtility.FloatArrayToFloat3(blueprint[i].scale);
				float3 blueRot = ConversionUtility.FloatArrayToFloat3(blueprint[i].rotation);
				float3 physicalLocation = (float3)(cubeQuaternion * bluePos) + rootPos;// + (blueprintSizeRotated / 2);
				//physicalLocation.x += blueprintSize.x / 2;
				physicalLocation += (float3)(cubeQuaternion * (correctionVec));
				//physicalLocation.y -= (float)(RobotCommands.blockSize * scale / 2);
				//float3 physicalScale = (float3)(cubeQuaternion * blueScale); // this actually over-rotates when combined with rotation
				float3 physicalScale = blueScale;
				float3 physicalRotation = (cubeQuaternion * Quaternion.Euler(blueRot)).eulerAngles;
#if DEBUG
				Logging.MetaLog($"Placing blueprint block at {physicalLocation} rot{physicalRotation} scale{physicalScale}");
				Logging.MetaLog($"Location math check original:{bluePos} rotated: {(float3)(cubeQuaternion * bluePos)} actualPos: {rootPos} result: {physicalLocation}");
				Logging.MetaLog($"Scale math check original:{blueScale} rotation: {(float3)cubeQuaternion.eulerAngles} result: {physicalScale}");
				Logging.MetaLog($"Rotation math check original:{blueRot} rotated: {(cubeQuaternion * Quaternion.Euler(blueRot))} result: {physicalRotation}");
#endif
				adjustedBlueprint[i] = new BlockJsonInfo
				{
					color = physicalColor,
					name = blueprint[i].name,
					position = ConversionUtility.Float3ToFloatArray(physicalLocation),
					rotation = ConversionUtility.Float3ToFloatArray(physicalRotation),
					scale = ConversionUtility.Float3ToFloatArray(physicalScale)
				};
			}
            return adjustedBlueprint;
        }

        public void AddReloadCommand()
        {
	        CommandBuilder.Builder("PixiReload", "Reloads the robot blueprints")
		        .Action(() => botprints = null).Build();
        }
    }
}