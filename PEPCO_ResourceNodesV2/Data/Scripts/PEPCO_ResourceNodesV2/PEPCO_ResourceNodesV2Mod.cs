using Digi;
using PEPCO.VectorThrustEngines.Sync;
using PEPCO_ResourceNodesV2.Sync;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;

namespace PEPCO_ResourceNodesV2
{

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    internal class PEPCO_ResourceNodesV2Mod : MySessionComponentBase
    {
        private const string MOD_NAME = "PEPCO_ResourceNodesV2";
        public Networking Networking = new Networking(12579);

        public PacketBlockSettings CachedPacketSettings;
        public static PEPCO_ResourceNodesV2Mod Instance { get; private set; }

        public bool IsPlayer = false;

        public override void LoadData()
        {
            Instance = this;
            Log.ModName = MOD_NAME;
            Log.AutoClose = true;

            Networking.Register();

            CachedPacketSettings = new PacketBlockSettings();

            IsPlayer = !(MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated);

        }

        protected override void UnloadData()
        {
            Networking?.Unregister();
            Networking = null;

            Log.Close();
            Instance = null;
        }

    }
}
