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

namespace SubmarineStuff
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false,
                                    "BallastTank")]
    public class MyBallastTankBlock : MyGameLogicComponent
    {

        IMyTerminalBlock block;

        private MyEntity _entity;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            block = (IMyTerminalBlock)Entity;
            _entity = Entity as MyEntity;

        }


        public override void UpdateOnceBeforeFrame()
        {

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;

        }

        public override void UpdateBeforeSimulation()
        {
            if (SubmarineStuffMod.Instance == null || !SubmarineStuffMod.Instance.IsPlayer)
                return;

            if (block?.CubeGrid?.Physics == null)
                return;

            try
            {



                Log.Info($"PercentUnderwater: {WaterModAPI.Entity_FluidDepth(_entity)}\n" + //0
                    $"(MyEntity)_entity is null: {_entity == null}"); //False

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


    }
}