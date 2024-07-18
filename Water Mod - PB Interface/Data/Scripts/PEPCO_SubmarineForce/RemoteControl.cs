using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Lights;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using Digi;
using Sandbox.Game.EntityComponents;
using static VRageMath.Base6Directions;
using static VRageMath.Base27Directions;
using SubmarineStuff;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game;
using VRage;
using Jakaria.API;
using VRage.Game.Entity;
using System.Text;

namespace SubmarineStuff
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), false)]
    public class RemoteControlLogic : MyGameLogicComponent
    {

        IMyRemoteControl block;
        IMyCubeGrid myCubeGrid;

        public Vector3 BuoyancyForce { get; set; }
        public Vector3 CenterOfBuoyancy { get; set; }
        public Vector3 FluidVelocity { get; set; }
        public float FluidDepth { get; set; }
        public float PercentUnderwater { get; set; }
        public float FluidPressure { get; set; }
        public float BuoyancyRatio { get; set; }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (WaterModAPI.Registered)
            {
                RemoteControlTerminalControls.DoOnce(ModContext);

                block = (IMyRemoteControl)Entity;
                myCubeGrid = block.CubeGrid;

                NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
            else
            {
                Log.Info("WaterModAPI not registered");
            }

        }

        public override void UpdateAfterSimulation10()
        {
            if (myCubeGrid?.Physics == null)
                return; // ignore ghost/projected grids

            // Error handling
            try
            {
                BuoyancyForce = WaterModAPI.Entity_BuoyancyForce((MyCubeGrid)myCubeGrid);
                CenterOfBuoyancy = WaterModAPI.Entity_CenterOfBuoyancy((MyCubeGrid)myCubeGrid);
                FluidVelocity = WaterModAPI.Entity_FluidVelocity((MyCubeGrid)myCubeGrid);
                FluidDepth = (float)WaterModAPI.Entity_FluidDepth((MyCubeGrid)myCubeGrid);
                PercentUnderwater = WaterModAPI.Entity_PercentUnderwater((MyCubeGrid)myCubeGrid);
                FluidPressure = WaterModAPI.Entity_FluidPressure((MyCubeGrid)myCubeGrid);
                BuoyancyRatio = BuoyancyForce.Length() / (myCubeGrid.Physics.Mass * myCubeGrid.Physics.Gravity.Length()) * 100;
            }
            catch (Exception e)
            {
                Log.Info("Error in SubmarineStuff: " + e.Message);
            }



        }

    }
}