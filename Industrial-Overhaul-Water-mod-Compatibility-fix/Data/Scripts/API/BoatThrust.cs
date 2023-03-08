using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
// using ProtoBuf;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using Jakaria.API;
using VRage.Utils;

namespace Propellers
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false, "SmallPropellerSmall", "SmallPropellerLarge", "MediumPropellerSmall", "MediumPropellerLarge", "LargePropellerSmall", "LargePropellerLarge", "SmallDuctedPropellerSmall", "SmallDuctedPropellerLarge", "LargeDuctedPropellerSmall", "LargeDuctedPropellerLarge")] //Edit these to your subtypes
    public class BoatThrust : MyGameLogicComponent
    {
        IMyThrust Propeller;
		
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {            
            Propeller = Entity as IMyThrust;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;            
        }

        public override void UpdateAfterSimulation100()
        {
            try
            {
				if (Propeller.CubeGrid.Physics != null)
                {
                    if (WaterAPI.IsUnderwater(Propeller.WorldMatrix.Translation))
                    {
                        Propeller.ThrustMultiplier = 2.0f;
                    }
                    else
                    {
                        Propeller.ThrustMultiplier = 2.0f;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}