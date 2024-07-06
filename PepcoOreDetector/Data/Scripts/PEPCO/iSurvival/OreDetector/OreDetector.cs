using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRage.Voxels;
using VRageMath;

namespace PEPCO.iSurvival.OreDetector
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeBlock), false, "OreDetectorBlock")]
    public class OreDetector : MyGameLogicComponent
    {
        private IMyTerminalBlock block;
        private IMyTerminalControlCombobox oreDropdown;
        private HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
        private List<MyVoxelMaterialDefinition> detectedOres = new List<MyVoxelMaterialDefinition>();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = Entity as IMyTerminalBlock;

            if (block != null)
            {
                MyAPIGateway.TerminalControls.CustomControlGetter += CustomControlGetter;
                MyAPIGateway.TerminalControls.CustomActionGetter += CustomActionGetter;
            }
        }

        public override void Close()
        {
            if (block != null)
            {
                MyAPIGateway.TerminalControls.CustomControlGetter -= CustomControlGetter;
                MyAPIGateway.TerminalControls.CustomActionGetter -= CustomActionGetter;
            }
        }

        private void CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block.BlockDefinition.SubtypeId == "OreDetectorBlock")
            {
                oreDropdown = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyTerminalBlock>("OreDetector_OreDropdown");
                oreDropdown.Title = MyStringId.GetOrCompute("Detected Ores");
                oreDropdown.Getter = (b) => 0;
                oreDropdown.Setter = (b, v) => { };
                oreDropdown.ComboBoxContent = FillOreDropdown;
                oreDropdown.SupportsMultipleBlocks = false;
                controls.Add(oreDropdown);

                UpdateDetectedOres();
            }
        }

        private void CustomActionGetter(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            // No custom actions required
        }

        private void FillOreDropdown(List<MyTerminalControlComboBoxItem> list)
        {
            for (int i = 0; i < detectedOres.Count; i++)
            {
                var item = new MyTerminalControlComboBoxItem
                {
                    Key = i,
                    Value = MyStringId.GetOrCompute(detectedOres[i].MinedOre)
                };
                list.Add(item);
            }
        }

        private void UpdateDetectedOres()
        {
            detectedOres.Clear();
            var playerPosition = block.GetPosition();

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyVoxelBase);

            foreach (var entity in entities)
            {
                var voxelBase = entity as IMyVoxelBase;
                if (voxelBase == null || voxelBase.Storage == null) continue;

                var voxelMap = voxelBase as MyVoxelBase;
                if (voxelMap == null) continue;

                Vector3D minWorldCoord = playerPosition - new Vector3D(100); // Detection radius
                Vector3D maxWorldCoord = playerPosition + new Vector3D(100);

                Vector3I minVoxelCoord = Vector3I.Floor((minWorldCoord - voxelBase.PositionLeftBottomCorner) / 1.0);
                Vector3I maxVoxelCoord = Vector3I.Ceiling((maxWorldCoord - voxelBase.PositionLeftBottomCorner) / 1.0);

                var materialData = new MyStorageData();
                materialData.Resize(Vector3I.One);

                for (int x = minVoxelCoord.X; x <= maxVoxelCoord.X; x += 2)
                {
                    for (int y = minVoxelCoord.Y; y <= maxVoxelCoord.Y; y += 2)
                    {
                        for (int z = minVoxelCoord.Z; z <= maxVoxelCoord.Z; z += 2)
                        {
                            var voxelCoord = new Vector3I(x, y, z);
                            voxelBase.Storage.ReadRange(materialData, MyStorageDataTypeFlags.Material, 0, voxelCoord, voxelCoord);
                            byte materialIdx = materialData.Material(0);

                            var material = MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIdx);
                            if (material != null && material.IsRare && !detectedOres.Contains(material))
                            {
                                detectedOres.Add(material);
                            }
                        }
                    }
                }
            }
        }
    }
}
