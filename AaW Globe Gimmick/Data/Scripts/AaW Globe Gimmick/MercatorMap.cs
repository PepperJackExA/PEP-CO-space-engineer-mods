using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using Digi;
using Sandbox.Game.Entities;
using static VRageRender.MyBillboard;
using System.Numerics;
using static Sandbox.Game.Entities.MyCubeGrid;
using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;
using Digi;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using VRage.Game.Entity; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.





namespace PEPCO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "MercatorMap")]
    public class MercatorMapLogic : MyGameLogicComponent
    {


        //Material for the GlobeGimmick
        private MyStringId Material = MyStringId.GetOrCompute("Square");

        private const float MAX_DISTANCE_SQ = 100 * 100; // player camera must be under this distance (squared) to see the subpart spinning
                                                         //Because I like the color red
        Color color = (Color.Red * 100);


        private IMyTerminalBlock block;
        private bool isRegistered = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyTerminalBlock)Entity;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            try
            {
                // If we are on a dedicated server, do nothing
                if (MyAPIGateway.Utilities.IsDedicated)
                {
                    Log.Info("This mod does nothing on dedicated servers.");
                    return;
                }


                // If the block is not ignore
                if (block?.CubeGrid?.Physics == null)
                {
                    Log.Info("Block or grid has no physics, skipping.");
                    return;
                }

                // Only register the GlobeGimmick once
                if (!isRegistered)
                {
                    isRegistered = GlobeGimmickSession.Instance.RegisterMap(block);
                }

                //Stop updating yourself
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}