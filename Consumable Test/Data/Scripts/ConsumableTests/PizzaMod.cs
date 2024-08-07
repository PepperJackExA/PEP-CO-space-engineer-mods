using Digi;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using VRage.Game.Entity;
using VRage.Utils;
using VRage.Game;
using VRage.ModAPI;
using Sandbox.ModAPI.Weapons;
using static VRage.Game.ObjectBuilders.ComponentSystem.MyObjectBuilder_PhysicsComponentDefinitionBase;

namespace ConsumableTests
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class PizzaMod : MySessionComponentBase
    {
        int tick = 0;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            //Test if this is a server
            if (!MyAPIGateway.Session.IsServer && !MyAPIGateway.Utilities.IsDedicated && !MyAPIGateway.Session.IsCameraUserControlledSpectator && MyAPIGateway.Session?.Player?.Character != null)
            {
                Log.Info("PizzaMod - This is not a server, PizzaMod will run.");
                SetUpdateOrder(MyUpdateOrder.BeforeSimulation);
            }
            else
            {
                Log.Info("PizzaMod - something isn't right, PizzaMod will not run.");
                SetUpdateOrder(MyUpdateOrder.NoUpdate);
            }
        }



        protected override void UnloadData()
        {

        }


        //update before simulation
        public override void UpdateBeforeSimulation()
        {


            if (tick++ == 60)
            {
                Log.Info    ("PizzaMod - Hello World!");
                tick = 0;
            }
        }
    }
}
