using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions.SessionComponents;

namespace PEPCO_Limited_Voxel_Placement
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Example_EditCharacterDef : MySessionComponentBase
    {

        public override void LoadData()
        {
            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                var blockDef = def as MyCubeBlockDefinition;
                if (blockDef != null)
                {
                    // ignore all CubeBlock and BatteryBlock types
                    if (blockDef.Id.TypeId == typeof(MyObjectBuilder_CubeBlock))
                        continue;
                    if (blockDef.Id.TypeId == typeof(MyObjectBuilder_BatteryBlock))
                        continue;

                    blockDef.VoxelPlacement = new VoxelPlacementOverride()
                    {
                        DynamicMode = new VoxelPlacementSettings()
                        {
                            PlacementMode = VoxelPlacementMode.OutsideVoxel,
                        },
                        StaticMode = new VoxelPlacementSettings()
                        {
                            PlacementMode = VoxelPlacementMode.OutsideVoxel,
                        },
                    };
                }
            }
        }   
    }
}
