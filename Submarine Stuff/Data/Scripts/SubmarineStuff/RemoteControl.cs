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

        Vector3D blockPosition;
        MyEntity myEntity;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void UpdateOnceBeforeFrame()
        {

            RemoteControlTerminalControls.DoOnce(ModContext);

            block = (IMyRemoteControl)Entity;
            myEntity = (MyEntity)Entity;
            if (block.CubeGrid?.Physics == null)
                return; // ignore ghost/projected grids

            Vector3 boyancyForce = WaterModAPI.Entity_BuoyancyForce((MyEntity)block);

            Log.Info($"WaterModAPI.Registered: {WaterModAPI.Registered}" +
                $"(MyEntity)block == null: {(MyEntity)block == null}" +
                $"boyancyForce: {boyancyForce}" +
                $"myEntity == null: {myEntity == null}" +
                $"boyancyForce: {WaterModAPI.Entity_BuoyancyForce(myEntity)}" +
                $"Grid: {block.CubeGrid.CustomName}");

        }

    }
}