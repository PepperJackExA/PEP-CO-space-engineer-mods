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

namespace PEPCO_Propulsion
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeBlock), false,
                                    "ElectricShipEngineMounting")]
    public class PEPCOWaterElectricShipEngineMounting : MyGameLogicComponent
    {

        

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {   
            Log.Info($"Me: {Entity.PositionComp.LocalMatrixRef.Translation}");
        }

        
    }
}