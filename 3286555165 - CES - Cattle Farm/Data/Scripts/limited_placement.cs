using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using System.Collections.Generic;
using VRage.Game;

namespace iSurvival.limitedplacement
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class BlockPlacementEnforcer : MySessionComponentBase
    {
        private static readonly HashSet<string> AllowedBaseBlocks = new HashSet<string> { "PlanterBox" };

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
        }

        private void OnEntityAdd(IMyEntity entity)
        {
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid != null)
            {
                grid.OnBlockAdded += OnBlockAdded;
            }
        }

        private void OnBlockAdded(IMySlimBlock block)
        {
            if (block != null && block.BlockDefinition.Id.SubtypeName == "Wheat_Plant")
            {
                IMySlimBlock baseBlock = GetBaseBlock(block);
                if (baseBlock == null || !AllowedBaseBlocks.Contains(baseBlock.BlockDefinition.Id.SubtypeName))
                {
                    block.CubeGrid.RazeBlock(block.Position);
                    MyAPIGateway.Utilities.ShowMessage("BlockPlacement", "This block can only be placed on specified base blocks.");
                }
            }
        }

        private IMySlimBlock GetBaseBlock(IMySlimBlock block)
        {
            Vector3I belowPosition = block.Position + Base6Directions.GetIntVector(Base6Directions.Direction.Down);
            return block.CubeGrid.GetCubeBlock(belowPosition);
        }
    }
}
