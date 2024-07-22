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

namespace PEPCO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "GlobeGimmick")]
    public class GlobeGimmickLogic : MyGameLogicComponent
    {


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
                    isRegistered = GlobeGimmickSession.Instance.RegisterGlobe(block);
                }
                
                //Stop updating yourself
                NeedsUpdate = MyEntityUpdateEnum.NONE;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}