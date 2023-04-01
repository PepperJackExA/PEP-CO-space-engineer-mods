using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions.SessionComponents;

namespace PEPCO_Limited_Voxel_Placement
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Limited_Voxel_Placement : MySessionComponentBase
    {

        public override void LoadData()
        {
            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                var blockDef = def as MyCubeBlockDefinition;
                if (blockDef != null)
                {
                    // ignore all CubeBlock and BatteryBlock types
                    if (blockDef.Id.TypeId == typeof(MyObjectBuilder_CubeBlock) || blockDef.Id.TypeId == typeof(MyObjectBuilder_Battery) || blockDef.Id.SubtypeId.ToString() == "BasicStaticDrill" || blockDef.Id.SubtypeId.ToString() == "AdvancedStaticDrill" || blockDef.Id.SubtypeId.ToString() == "StaticDrill")
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
