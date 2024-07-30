using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using VRage.Utils;
using System;
using VRageMath;

namespace PEPCO.Template
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SessionComponent : MySessionComponentBase
    {
        public override void LoadData()
        {
            ConfigSettings.Load();
            MyAPIGateway.Utilities.MessageEntered += CommandHandler.OnMessageEntered;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= CommandHandler.OnMessageEntered;
        }

        public override void UpdateBeforeSimulation()
        {
            // Add your session processing logic here
        }
    }
}
